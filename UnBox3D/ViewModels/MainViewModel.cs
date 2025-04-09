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

        public ObservableCollection<MeshSummary> Meshes { get; } = new();

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
                    Meshes.Add(new MeshSummary(mesh));
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

            var installWindow = new Views.LoadingWindow
            {
                StatusHint = "Installing Blender. Please wait..."
            };
            installWindow.Show();

            var installProgress = new Progress<double>(value =>
            {
                installWindow.UpdateProgress(value * 100);
                installWindow.UpdateStatus($"Installing Blender... {Math.Round(value * 100)}%");
            });

            await _blenderInstaller.CheckAndInstallBlender(installProgress);
            installWindow.Close();

            if (!_blenderInstaller.IsBlenderInstalled())
            {
                await ShowWpfMessageBoxAsync(
                    "Blender is required to unfold models but was not found. Please install Blender before proceeding.",
                    "Missing Dependency", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            else
            {

                var loadingWindow = new Views.LoadingWindow
                {
                    StatusHint = "This may take several minutes depending on model complexity"
                };
                loadingWindow.Show();

                if (_fileSystem == null || _blenderIntegration == null)
                {
                    loadingWindow.Close();
                    await ShowWpfMessageBoxAsync("Internal error: dependencies not initialized.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (PageWidth == 0 || PageHeight == 0)
                {
                    loadingWindow.Close();
                    await ShowWpfMessageBoxAsync("Page Dimensions cannot be 0.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                using SaveFileDialog saveFileDialog = new()
                {
                    Title = "Save your unfolded file",
                    Filter = "SVG Files|*.svg|PDF Files|*.pdf",
                    FileName = "MyUnfoldedFile"
                };

                if (saveFileDialog.ShowDialog() != DialogResult.OK)
                {
                    loadingWindow.Close();
                    return;
                }

                string filePath = saveFileDialog.FileName;
                string ext = Path.GetExtension(filePath).ToLowerInvariant();
                string? userSelectedPath = Path.GetDirectoryName(filePath);

                if (string.IsNullOrEmpty(userSelectedPath))
                {
                    loadingWindow.Close();
                    await ShowWpfMessageBoxAsync("Unable to determine the selected directory.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string newFileName = Path.GetFileNameWithoutExtension(filePath);
                string format = ext == ".pdf" ? "PDF" : "SVG";

                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string outputDirectory = _fileSystem.CombinePaths(baseDir, "UnfoldedOutputs");
                string scriptPath = _fileSystem.CombinePaths(baseDir, "Scripts", "unfolding_script.py");

                if (!_fileSystem.DoesDirectoryExists(outputDirectory))
                {
                    _fileSystem.CreateDirectory(outputDirectory);
                }

                CleanupUnfoldedFolder(outputDirectory);

                double incrementWidth = PageWidth;
                double incrementHeight = PageHeight;
                bool success = false;
                string errorMessage = "";
                int iteration = 0;

                loadingWindow.UpdateStatus("Preparing Blender environment...");
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
                            loadingWindow.UpdateStatus($"Retrying with new dimensions: {incrementWidth} x {incrementHeight}");
                            await DispatcherHelper.DoEvents();
                        }
                        else
                        {
                            loadingWindow.Close();

                            await loadingWindow.Dispatcher.InvokeAsync(() =>
                            {
                                System.Windows.Application.Current.MainWindow?.Activate();
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
                }

                loadingWindow.UpdateStatus("Processing unfolded panels...");
                await DispatcherHelper.DoEvents();

                string[] svgPanelFiles = Directory.GetFiles(outputDirectory, "*.svg");
                int totalPanels = svgPanelFiles.Length;

                for (int i = 0; i < totalPanels; i++)
                {
                    string svgFile = svgPanelFiles[i];
                    loadingWindow.UpdateStatus($"Processing panel {i + 1} of {totalPanels}");
                    loadingWindow.UpdateProgress(50 + ((double)i / totalPanels * 30));
                    await DispatcherHelper.DoEvents();

                    await Task.Run(() => SVGEditor.ExportSvgPanels(svgFile, outputDirectory, newFileName, i,
                        PageWidth * 1000f, PageHeight * 1000f));
                }

                loadingWindow.UpdateStatus("Exporting final files...");
                loadingWindow.UpdateProgress(80);
                await DispatcherHelper.DoEvents();

                if (format == "SVG")
                {
                    string[] svgFiles = Directory.GetFiles(outputDirectory, $"{newFileName}*.svg");
                    int fileCount = svgFiles.Length;

                    for (int i = 0; i < fileCount; i++)
                    {
                        string source = svgFiles[i];
                        string suffix = source.Substring(source.IndexOf(newFileName) + newFileName.Length);
                        string destination = Path.Combine(userSelectedPath, newFileName + suffix);

                        loadingWindow.UpdateStatus($"Exporting file {i + 1} of {fileCount}");
                        loadingWindow.UpdateProgress(80 + ((double)i / fileCount * 20));
                        await DispatcherHelper.DoEvents();

                        File.Move(source, destination, overwrite: true); // Optionally wrap in _fileSystem
                    }
                }
                else if (format == "PDF")
                {
                    string pdfFile = Path.Combine(outputDirectory, $"{newFileName}.pdf");
                    string destination = Path.Combine(userSelectedPath, $"{newFileName}.pdf");
                    File.Move(pdfFile, destination, overwrite: true);
                }

                loadingWindow.UpdateStatus("Cleaning up temporary files...");
                loadingWindow.UpdateProgress(100);
                await DispatcherHelper.DoEvents();
                CleanupUnfoldedFolder(outputDirectory);

                loadingWindow.Close();

                await ShowWpfMessageBoxAsync($"{format} file has been exported successfully!",
                    "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
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
            string newName = PromptForNewName(mesh.Name);
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
