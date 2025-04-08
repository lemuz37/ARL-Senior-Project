using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenTK.Mathematics;
using System.Collections.ObjectModel;
using UnBox3D.Models;
using UnBox3D.Utils;
using UnBox3D.Rendering;
using System.Diagnostics;
using System.IO;
using UnBox3D.Views;


namespace UnBox3D.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
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

        /* IMPORTANT:
         * When you begin simplifying the model, EXPORT that simplified version to
         * the ImportedModels dir and OVERWRITE the file because the unfolding script will use
         * the ImportedModels dir and is not responsible for keeping track if its been simplified or not.
        */

        // Don't call this function. Reference the _importedFilePath if you want to get access to the ImportedModels dir
        private string EnsureImportDirectory(string filePath)
        {
            string importDirectory = _fileSystem.CombinePaths(AppDomain.CurrentDomain.BaseDirectory, "ImportedModels");

            if (!_fileSystem.DoesDirectoryExists(importDirectory))
            {
                _fileSystem.CreateDirectory(importDirectory);
            }

            string destinationPath = _fileSystem.CombinePaths(importDirectory, Path.GetFileName(filePath));
            File.Copy(filePath, destinationPath, overwrite:true);

            return destinationPath;
        }

        private async Task ProcessUnfolding(string inputModelPath)
        {
            Debug.WriteLine("Input model is coming from: " + inputModelPath);
            var loadingWindow = new Views.LoadingWindow();

            // Ensure Blender is installed before continuin
            await _blenderInstaller.CheckAndInstallBlender();

            if (_fileSystem == null || _blenderIntegration == null)
            {
                MessageBox.Show("Internal error: dependencies not initialized.");
                return;
            }

            if (PageWidth == 0 || PageHeight == 0)
            {
                MessageBox.Show($"Page Dimensions cannot be 0.");
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
                MessageBox.Show("Unable to determine the selected directory.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            // In case of a crash, or force exit, some files remain inside and will cause incrorrect manipulation
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
                        break;
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

                    string previewFolder = Path.Combine(outputDirectory, $"{newFileName}_Export");

                    // Convert all SVGs to PNGs for preview
                    string[] svgFiles = Directory.GetFiles(previewFolder, "*.svg");
                    List<string> pngFiles = new List<string>();

                    foreach (string svgFile in svgFiles)
                    {
                        string pngPath = Path.ChangeExtension(svgFile, ".png");
                        SVGToPNGConverter.Convert(svgFile, pngPath);

                        // Ensure the file was actually created before adding it
                        if (File.Exists(pngPath))
                        {
                            pngFiles.Add(pngPath);
                        }
                        else
                        {
                            Debug.WriteLine($"WARNING: PNG not created for {svgFile}");
                        }
                    }

                    // If no previews were successfully created, skip preview
                    if (pngFiles.Count == 0)
                    {
                        MessageBox.Show("No preview files were generated. Export may have failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Delay to give file system a moment to flush writes (optional)
                    await Task.Delay(200);

                    // Show preview window with PNGs
                    var previewWindow = new SvgPreviewWindow(pngFiles);
                    bool? confirmed = previewWindow.ShowDialog();

                    if (confirmed != true)
                    {
                        MessageBox.Show("Export cancelled by user.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }


                    if (confirmed != true)  
                    {
                        MessageBox.Show("Export cancelled by user.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }


                    loadingWindow.UpdateStatus("Exporting SVG folder...");
                    loadingWindow.UpdateProgress(95);
                    await DispatcherHelper.DoEvents();

                    string sourceFolder = Path.Combine(outputDirectory, $"{newFileName}_Export");
                    string destinationFolder = Path.Combine(userSelectedPath, $"{newFileName}_Export");

                    if (Directory.Exists(destinationFolder))
                    {
                        Directory.Delete(destinationFolder, true);
                    }

                    Directory.Move(sourceFolder, destinationFolder);
                }

                else if (format == "PDF")
                {
                    loadingWindow.UpdateStatus ("Exporting PDF file...");
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
                MessageBox.Show($"{format} file has been exported successfully!", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                loadingWindow.Close();
                MessageBox.Show(errorMessage, "Error Processing File", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show($"An error occurred during cleanup: {ex.Message}", "Cleanup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        [RelayCommand]
        private void ExportUnfoldModel()
        {
            if (string.IsNullOrEmpty(_importedFilePath))
            {
                MessageBox.Show("No model imported to unfold.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            //MessageBox.Show($"WidthCall:  {PageWidth}, HeightCall:  {PageHeight}");
            ProcessUnfolding(_importedFilePath);
        }

        // Command to reset the view
        [RelayCommand]
        private void ResetView()
        {
            MessageBox.Show("Resetting the view!");
        }

        // Command for hierarchy visibility
        [RelayCommand]
        private void ToggleHierarchy()
        {
            HierarchyVisible = !HierarchyVisible;
        }

        // Command to show About dialog
        [RelayCommand]
        private void About()
        {
            MessageBox.Show("UnBox3D - A 3D Model Viewer");
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
        private void ExportMesh(IAppMesh mesh)
        {
            string exportPath = PromptForSaveLocation();
            //ModelExporter.Export(mesh, exportPath);
        }

        // Helper methods (You need to implement UI prompts)
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


        // Mesh Simplification Commands
        [RelayCommand]
        private void SimplifyQEM()
        {
            MessageBox.Show("QEM Simplification triggered!");
            // Call QEM simplification logic here
        }

        [RelayCommand]
        private void SimplifyEdgeCollapse()
        {
            MessageBox.Show("Edge Collapse Simplification triggered!");
            // Call Edge Collapse simplification logic here
        }

        [RelayCommand]
        private void SimplifyDecimation()
        {
            MessageBox.Show("Decimation Simplification triggered!");
            // Call Decimation logic here
        }

        [RelayCommand]
        private void SimplifyAdaptiveDecimation()
        {
            MessageBox.Show("Adaptive Decimation Simplification triggered!");
            // Call Adaptive Decimation logic here
        }

        // Command to exit the application
        [RelayCommand]
        private void Exit()
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
