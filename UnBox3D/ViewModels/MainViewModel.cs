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
                List<IAppMesh> importedMeshes = _modelImporter.ImportModel(filePath);

                foreach (var mesh in importedMeshes)
                {
                    _sceneManager.AddMesh(mesh);
                }

                ProcessUnfolding(filePath);
            }
        }

        private async Task ProcessUnfolding(string inputModelPath)
        {
            // Ensure Blender is installed before continuing
            await _blenderInstaller.CheckAndInstallBlender();
            
            if (_fileSystem == null || _blenderIntegration == null)
            {
                MessageBox.Show("Internal error: dependencies not initialized.");
                return;
            }

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            Debug.WriteLine("baseDir: " + baseDir);
            string outputDirectory = _fileSystem.CombinePaths(baseDir, "UnfoldedOutputs");
            Debug.WriteLine("outputDir: " + outputDirectory);

            if (!_fileSystem.DoesDirectoryExists(outputDirectory))
            {
                _fileSystem.CreateDirectory(outputDirectory);
            }

            string scriptPath = _fileSystem.CombinePaths(baseDir, "Scripts", "unfolding_script.py");
            string tempFileName = "HardCodedTestingSVG25mx25m";
            Debug.WriteLine("scriptPath:" + scriptPath);

            // TODO: Eventually set up the page increment shenanigans
            // TODO: SVG stuff (correcting scale)
            // TODO: UI so its not hardcoded
            // TODO: Create import dir to store global inputModelPath (obj)
            // TODO: Create export/unfold button separately and read from import dir obj

            bool success = _blenderIntegration.RunBlenderScript(
                inputModelPath, outputDirectory, scriptPath,
                tempFileName, 25.0, 25.0, "SVG", out string errorMessage);

            if (success)
            {
                MessageBox.Show($"Unfolding successful! Output saved to: {outputDirectory}", "Success");
            }
            else
            {
                MessageBox.Show($"Unfolding failed: {errorMessage}");
            }

            // Let the user select a directory for saving the files
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Title = "Save your SVG file";
                saveFileDialog.Filter = "SVG Files|*.svg";
                saveFileDialog.FileName = "MyUnfoldedFile.svg";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string? userSelectedPath = Path.GetDirectoryName(saveFileDialog.FileName);
                    if (string.IsNullOrEmpty(userSelectedPath))
                    {
                        MessageBox.Show("Unable to determine the selected directory.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    string newFileName = Path.GetFileNameWithoutExtension(saveFileDialog.FileName);
                    
                    // Copy all SVG files from the temp directory to the user's selected path
                    string[] svgFiles = Directory.GetFiles(outputDirectory, $"{tempFileName}*.svg");

                    foreach (string svgFile in svgFiles)
                    {
                        // Determine suffix of temp file
                        int index = svgFile.IndexOf(tempFileName) + tempFileName.Length;
                        string fileSuffix = svgFile.Substring(index);

                        string destinationFilePath = Path.Combine(userSelectedPath, newFileName + fileSuffix);
                        File.Move(svgFile, destinationFilePath);
                    }

                    MessageBox.Show("Files have been exported successfully!", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

                }
                else
                {
                    CleanupUnfoldedFolder(outputDirectory);
                }
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
        
        [RelayCommand]
        private void OpenStructureSupportWindow()
        {
   
            MessageBox.Show("SS window opened");
            StructureSupportWindow window = new StructureSupportWindow();
            window.Show();
        }

        // Command to exit the application
        [RelayCommand]
        private void Exit()
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
