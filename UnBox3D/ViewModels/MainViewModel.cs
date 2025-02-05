using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenTK.Mathematics;
using System.Collections.ObjectModel;
using UnBox3D.Models;

namespace UnBox3D.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ModelLoader _modelLoader;

        public ObservableCollection<Vector3> Vertices { get; } = new();

        public MainViewModel()
        {
            _modelLoader = new ModelLoader();
        }

        [RelayCommand]
        private void LoadModel()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "3D Models (*.obj;*.fbx;*.dae)|*.obj;*.fbx;*.dae"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                Vertices.Clear();
                if (_modelLoader.LoadModel(openFileDialog.FileName))
                {
                    foreach (var vertex in _modelLoader.Vertices)
                    {
                        Vertices.Add(vertex);
                    }
                }
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
