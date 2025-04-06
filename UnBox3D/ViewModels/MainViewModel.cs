using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenTK.Mathematics;
using System.Collections.ObjectModel;
using UnBox3D.Models;
using UnBox3D.Utils;
using UnBox3D.Rendering;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace UnBox3D.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        #region Fields & Properties

        private readonly ISettingsManager _settingsManager;
        private readonly ISceneManager _sceneManager;
        private readonly ModelImporter _modelImporter;
        private readonly IFileSystem _fileSystem;
        private readonly BlenderIntegration _blenderIntegration;
        private readonly IBlenderInstaller _blenderInstaller;
        private string _importedFilePath; // Global filepath that should be referenced when simplifying

        [ObservableProperty]
        private IAppMesh selectedMesh;

        [ObservableProperty]
        private bool hierarchyVisible = true;

        [ObservableProperty]
        private float pageWidth = 25.0f;

        [ObservableProperty]
        private float pageHeight = 25.0f;

        public ObservableCollection<IAppMesh> Meshes => _sceneManager.GetMeshes();

        #endregion

        #region Constructor

        public MainViewModel(ISettingsManager settingsManager, ISceneManager sceneManager,
            IFileSystem fileSystem, BlenderIntegration blenderIntegration,
            IBlenderInstaller blenderInstaller)
        {
            _settingsManager = settingsManager;
            _sceneManager = sceneManager;
            _fileSystem = fileSystem;
            _blenderIntegration = blenderIntegration;
            _blenderInstaller = blenderInstaller;
            _modelImporter = new ModelImporter(_settingsManager);
        }

        #endregion

        #region Model Import Methods

        [RelayCommand]
        private void ImportObjModel()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "3D Models (*.obj;)|*.obj;"
            };

            // Show the dialog and check if the result is true
            bool? result = openFileDialog.ShowDialog();
            if (result == true)
            {
                string filePath = openFileDialog.FileName;
                _importedFilePath = EnsureImportDirectory(filePath);
                List<IAppMesh> importedMeshes = _modelImporter.ImportModel(_importedFilePath);

                foreach (var mesh in importedMeshes)
                {
                    _sceneManager.AddMesh(mesh);
                }
            }
        }

        // Don't call this function directly.
        // Reference _importedFilePath if you want access to the ImportedModels directory.
        private string EnsureImportDirectory(string filePath)
        {
            string importDirectory = _fileSystem.CombinePaths(AppDomain.CurrentDomain.BaseDirectory, "ImportedModels");

            if (!_fileSystem.DoesDirectoryExists(importDirectory))
            {
                _fileSystem.CreateDirectory(importDirectory);
            }

            string destinationPath = _fileSystem.CombinePaths(importDirectory, Path.GetFileName(filePath));
            File.Copy(filePath, destinationPath, overwrite: true);

            return destinationPath;
        }

        #endregion

        #region Unfolding Process Methods

        private async Task ProcessUnfolding(string inputModelPath)
        {
            Debug.WriteLine("Input model is coming from: " + inputModelPath);
            var loadingWindow = new Views.LoadingWindow();

            // Ensure Blender is installed before continuing
            await _blenderInstaller.CheckAndInstallBlender();

            if (_fileSystem == null || _blenderIntegration == null)
            {
                await ShowWpfMessageBoxAsync("Internal error: dependencies not initialized.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (PageWidth == 0 || PageHeight == 0)
            {
                await ShowWpfMessageBoxAsync("Page Dimensions cannot be 0.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Let the user select a directory for saving the files
            using SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Save your unfolded file";
            saveFileDialog.Filter = "SVG Files|*.svg|PDF Files|*.pdf";
            saveFileDialog.FileName = "MyUnfoldedFile";

            if (saveFileDialog.ShowDialog() != DialogResult.OK) return;

            // Extract user selection
            string filePath = saveFileDialog.FileName;
            string ext = Path.GetExtension(filePath).ToLowerInvariant();
            string? userSelectedPath = Path.GetDirectoryName(filePath);

            if (string.IsNullOrEmpty(userSelectedPath))
            {
                await ShowWpfMessageBoxAsync("Unable to determine the selected directory.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string newFileName = Path.GetFileNameWithoutExtension(filePath);
            string format = ext == ".pdf" ? "PDF" : "SVG";

            // Set up output directories                
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string outputDirectory = _fileSystem.CombinePaths(baseDir, "UnfoldedOutputs");

            if (!_fileSystem.DoesDirectoryExists(outputDirectory))
            {
                _fileSystem.CreateDirectory(outputDirectory);
            }
            // In case of a crash or force exit, some files may remain and cause incorrect manipulation
            CleanupUnfoldedFolder(outputDirectory);

            string scriptPath = _fileSystem.CombinePaths(baseDir, "Scripts", "unfolding_script.py");
            double incrementWidth = PageWidth;
            double incrementHeight = PageHeight;

            bool success = false;
            string errorMessage = "";
            int iteration = 0;

            loadingWindow.Show();

            loadingWindow.UpdateStatus("Checking Blender installation...");
            await DispatcherHelper.DoEvents();
            loadingWindow.UpdateStatus("Preparing for file export...");
            await DispatcherHelper.DoEvents();
            loadingWindow.UpdateStatus("Setting up processing environment...");
            await DispatcherHelper.DoEvents();

            while (!success)
            {
                iteration++;
                loadingWindow.UpdateStatus($"Processing with Blender (Attempt {iteration})...");
                loadingWindow.UpdateProgress((double)iteration / 100 * 50);
                await DispatcherHelper.DoEvents();

                success = await Task.Run(() => _blenderIntegration.RunBlenderScript(
                    inputModelPath, outputDirectory, scriptPath,
                    newFileName, incrementWidth, incrementHeight, format, out errorMessage));

                if (!success)
                {
                    if (errorMessage.Contains("continue"))
                    {
                        incrementWidth++;
                        incrementHeight++;
                        loadingWindow.UpdateStatus($"Adjusting dimensions and retrying ({incrementWidth}x{incrementHeight})...");
                        await DispatcherHelper.DoEvents();
                        Debug.WriteLine($"errorME: {errorMessage} {incrementWidth} {incrementHeight}");
                    }
                    else
                    {
                        Debug.WriteLine($"break: {errorMessage} {incrementWidth} {incrementHeight}");
                        loadingWindow.Close();

                        await loadingWindow.Dispatcher.InvokeAsync(() =>
                        {
                            // Activate the main window to ensure it has focus
                            System.Windows.Application.Current.MainWindow.Activate();
                            // Show the MessageBox using DefaultDesktopOnly to force it on top
                            System.Windows.MessageBox.Show(
                                errorMessage,
                                "Error Processing File",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error,
                                MessageBoxResult.OK,
                                System.Windows.MessageBoxOptions.DefaultDesktopOnly);
                        });

                        return;
                    }
                }
                Debug.WriteLine($"script executed successfully: {errorMessage} {incrementWidth} {incrementHeight}");
            }
            Debug.WriteLine($"incW: {incrementWidth} incH {incrementHeight}");
            Debug.WriteLine($"PW: {PageWidth} PH: {PageHeight}");

            if (success)
            {
                loadingWindow.UpdateStatus("Processing SVG panels...");
                await DispatcherHelper.DoEvents();
                string[] svgPanelFiles = Directory.GetFiles(outputDirectory, "*.svg");
                int totalPanels = svgPanelFiles.Length;
                int processedPanels = 0;

                foreach (string svgFile in svgPanelFiles)
                {
                    loadingWindow.UpdateStatus($"Processing panel {processedPanels + 1} of {totalPanels}");
                    loadingWindow.UpdateProgress(50 + ((double)processedPanels / totalPanels * 30));
                    await DispatcherHelper.DoEvents();

                    await Task.Run(() => SVGEditor.ExportSvgPanels(svgFile, outputDirectory, newFileName, processedPanels,
                        PageWidth * 1000f, PageHeight * 1000f));

                    processedPanels++;
                }

                loadingWindow.UpdateStatus("Exporting final files...");
                loadingWindow.UpdateProgress(80);
                await DispatcherHelper.DoEvents();

                if (format == "SVG")
                {
                    string[] svgFiles = Directory.GetFiles(outputDirectory, $"{newFileName}*.svg");
                    int fileCount = svgFiles.Length;
                    int filesMoved = 0;

                    foreach (string svgFile in svgFiles)
                    {
                        loadingWindow.UpdateStatus($"Exporting file {filesMoved + 1} of {fileCount}");
                        loadingWindow.UpdateProgress(80 + ((double)filesMoved / fileCount * 20));
                        await DispatcherHelper.DoEvents();

                        string fileSuffix = svgFile.Substring(svgFile.IndexOf(newFileName) + newFileName.Length);
                        string destinationFilePath = Path.Combine(userSelectedPath, newFileName + fileSuffix);
                        File.Move(svgFile, destinationFilePath, overwrite: true);

                        filesMoved++;
                    }
                }
                else if (format == "PDF")
                {
                    loadingWindow.UpdateStatus("Exporting PDF file...");
                    loadingWindow.UpdateProgress(95);
                    await DispatcherHelper.DoEvents();

                    string pdfFile = Path.Combine(outputDirectory, $"{newFileName}.pdf");
                    string destinationFilePath = Path.Combine(userSelectedPath, $"{newFileName}.pdf");
                    File.Move(pdfFile, destinationFilePath, overwrite: true);
                }

                loadingWindow.UpdateStatus("Cleaning up temporary files...");
                loadingWindow.UpdateProgress(100);
                await DispatcherHelper.DoEvents();
                CleanupUnfoldedFolder(outputDirectory);

                loadingWindow.Close();
                await ShowWpfMessageBoxAsync($"{format} file has been exported successfully!", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                loadingWindow.Close();
                await ShowWpfMessageBoxAsync(errorMessage, "Error Processing File", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CleanupUnfoldedFolder(string folderPath)
        {
            try
            {
                string[] files = Directory.GetFiles(folderPath);
                foreach (string file in files)
                {
                    File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                // Synchronously show an error if cleanup fails.
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    System.Windows.Application.Current.MainWindow.Activate();
                    System.Windows.MessageBox.Show(
                        $"An error occurred during cleanup: {ex.Message}",
                        "Cleanup Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error,
                        MessageBoxResult.OK,
                        System.Windows. MessageBoxOptions.DefaultDesktopOnly);
                });
            }
        }

        #endregion

        #region Relay Commands

        [RelayCommand]
        private async Task ExportUnfoldModel()
        {
            if (string.IsNullOrEmpty(_importedFilePath))
            {
                await ShowWpfMessageBoxAsync("No model imported to unfold.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            await ProcessUnfolding(_importedFilePath);
        }

        [RelayCommand]
        private async void ResetView()
        {
            await ShowWpfMessageBoxAsync("Resetting the view!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private void ToggleHierarchy()
        {
            HierarchyVisible = !HierarchyVisible;
        }

        [RelayCommand]
        private async void About()
        {
            await ShowWpfMessageBoxAsync("UnBox3D - A 3D Model Viewer", "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private void RenameMesh(IAppMesh mesh)
        {
            string newName = PromptForNewName(mesh.GetName());
            //mesh.SetName(newName);
        }

        [RelayCommand]
        private void DeleteMesh(IAppMesh mesh)
        {
            _sceneManager.DeleteMesh(mesh);
        }

        [RelayCommand]
        private async void ExportMesh(IAppMesh mesh)
        {
            string exportPath = PromptForSaveLocation();
            //ModelExporter.Export(mesh, exportPath);
            await ShowWpfMessageBoxAsync($"Exporting mesh to: {exportPath}", "Export Mesh", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Mesh Simplification Commands
        [RelayCommand]
        private async void SimplifyQEM()
        {
            await ShowWpfMessageBoxAsync("QEM Simplification triggered!", "Simplification", MessageBoxButton.OK, MessageBoxImage.Information);
            // Call QEM simplification logic here
        }

        [RelayCommand]
        private async void SimplifyEdgeCollapse()
        {
            await ShowWpfMessageBoxAsync("Edge Collapse Simplification triggered!", "Simplification", MessageBoxButton.OK, MessageBoxImage.Information);
            // Call Edge Collapse simplification logic here
        }

        [RelayCommand]
        private async void SimplifyDecimation()
        {
            await ShowWpfMessageBoxAsync("Decimation Simplification triggered!", "Simplification", MessageBoxButton.OK, MessageBoxImage.Information);
            // Call Decimation logic here
        }

        [RelayCommand]
        private async void SimplifyAdaptiveDecimation()
        {
            await ShowWpfMessageBoxAsync("Adaptive Decimation Simplification triggered!", "Simplification", MessageBoxButton.OK, MessageBoxImage.Information);
            // Call Adaptive Decimation logic here
        }

        [RelayCommand]
        private void Exit()
        {
            System.Windows.Application.Current.Shutdown();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Shows a WPF MessageBox asynchronously using the UI Dispatcher.
        /// </summary>
        private static async Task ShowWpfMessageBoxAsync(string message, string title, MessageBoxButton button, MessageBoxImage image)
        {
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                // Activate main window so the MessageBox appears on top.
                System.Windows.Application.Current.MainWindow.Activate();
                System.Windows.MessageBox.Show(
                    message,
                    title,
                    button,
                    image,
                    MessageBoxResult.OK,
                    System.Windows.MessageBoxOptions.DefaultDesktopOnly);
            });
        }

        private string PromptForNewName(string currentName)
        {
            return Microsoft.VisualBasic.Interaction.InputBox("Enter new name:", "Rename Mesh", currentName);
        }

        private string PromptForSaveLocation()
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "3D Models (*.obj)|*.obj"
            };
            return saveFileDialog.ShowDialog() == true ? saveFileDialog.FileName : null;
        }

        #endregion
    }
}
