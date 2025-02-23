using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenTK.Mathematics;
using System.Collections.ObjectModel;
using UnBox3D.Models;
using UnBox3D.Utils;
using UnBox3D.Rendering;

namespace UnBox3D.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ISettingsManager _settingsManager;
        private readonly ISceneManager _sceneManager;
        private readonly ModelImporter _modelImporter;

        [ObservableProperty]
        private IAppMesh selectedMesh;
        [ObservableProperty]
        private bool hierarchyVisible = true;

        public ObservableCollection<IAppMesh> Meshes => _sceneManager.GetMeshes();


        public MainViewModel(ISettingsManager settingsManager, ISceneManager sceneManager)
        {
            _settingsManager = settingsManager;
            _sceneManager = sceneManager;
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

        // Command to exit the application
        [RelayCommand]
        private void Exit()
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
