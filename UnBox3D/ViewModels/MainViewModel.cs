using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenTK.Mathematics;
using System.Collections.ObjectModel;
using UnBox3D.Models;
using UnBox3D.Rendering;

namespace UnBox3D.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
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
                //_sceneManager.AddMeshes(ModelImporter.ImportModel(filePath));

                //foreach (var mesh in _sceneManager.GetMeshes())
                //{
                //    Meshes.Add(mesh);
                //}
            }
        }


        // Command to reset the view
        [RelayCommand]
        private void ResetView()
        {
            MessageBox.Show("Resetting the view!");
        }

        // Command for application preferences
        [RelayCommand]
        private void Preferences()
        {
            MessageBox.Show("Opening Preferences!");
        }

        // Command to show About dialog
        [RelayCommand]
        private void About()
        {
            MessageBox.Show("UnBox3D - A 3D Model Viewer");
        }

        // Command to exit the application
        [RelayCommand]
        private void Exit()
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
