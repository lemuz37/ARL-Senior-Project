using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenTK.Mathematics;
using System.Collections.ObjectModel;
using UnBox3D.Models;
using UnBox3D.Utils;
using UnBox3D.Rendering;
using System.Diagnostics;
using System.IO;

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
            // Ensure Blender is installed before continuing
            await _blenderInstaller.CheckAndInstallBlender();

            if (_fileSystem == null || _blenderIntegration == null)
            {
                MessageBox.Show("Internal error: dependencies not initialized.");
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

            string scriptPath = _fileSystem.CombinePaths(baseDir, "Scripts", "unfolding_script.py");

            // TODO: Eventually set up the page increment shenanigans
            // TODO: SVG stuff (correcting scale)
            // TODO: UI so its not hardcoded

            bool success = _blenderIntegration.RunBlenderScript(
                inputModelPath, outputDirectory, scriptPath,
                newFileName, 25.0, 25.0, format, out string errorMessage);

            if (!success)
            {
                MessageBox.Show($"Unfolding failed: {errorMessage}");
                return;
            }

            if (format == "SVG")
            {
                string[] svgFiles = Directory.GetFiles(outputDirectory, $"{newFileName}*.svg");

                foreach (string svgFile in svgFiles)
                {
                    string fileSuffix = svgFile.Substring(svgFile.IndexOf(newFileName) + newFileName.Length);
                    string destinationFilePath = Path.Combine(userSelectedPath, newFileName + fileSuffix);
                    File.Move(svgFile, destinationFilePath, overwrite: true);
                }
            }
            else if (format == "PDF")
            {
                string pdfFile = Path.Combine(outputDirectory, $"{newFileName}.pdf");

                string destinationFilePath = Path.Combine(userSelectedPath, $"{newFileName}.pdf");
                File.Move(pdfFile, destinationFilePath, overwrite: true);
            }

            MessageBox.Show($"{format} file has been exported successfully!", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            CleanupUnfoldedFolder(outputDirectory);
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
