using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using UnBox3D.Models;
using UnBox3D.Rendering;
using UnBox3D.Utils;
using System.Windows;

namespace UnBox3D.ViewModels
{
    public partial class ImportExportViewModel : ObservableObject
    {
        private readonly ISettingsManager _settingsManager;
        private readonly ILogger _logger;
        private readonly IFileSystem _fileSystem;
        private readonly ISceneManager _sceneManager;
        private readonly ModelExporter _modelExporter;
        private readonly ModelImporter _modelImporter;

        public ObservableCollection<MeshSummary> Meshes { get; } = new();

        public ImportExportViewModel(ISettingsManager settingsManager, ILogger logger, IFileSystem fileSystem,
                                     ISceneManager sceneManager, ModelExporter modelExporter)
        {
            _settingsManager = settingsManager;
            _logger = logger;
            _fileSystem = fileSystem;
            _sceneManager = sceneManager;
            _modelExporter = modelExporter;
            _modelImporter = new ModelImporter(_settingsManager);
        }

        public void ImportObjModel()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "3D Models (*.obj;)|*.obj;"
            };
            bool? result = openFileDialog.ShowDialog();
            if (result == true)
            {
                string filePath = openFileDialog.FileName;
                List<IAppMesh> importedMeshes = _modelImporter.ImportModel(filePath);

                foreach (var mesh in importedMeshes)
                {
                    _sceneManager.AddMesh(mesh);
                    Meshes.Add(new MeshSummary(mesh));
                }
            }
        }

        public async Task ExportAllMeshesAsync()
        {
            string? path = PromptForSaveLocation();
            if (string.IsNullOrEmpty(path))
                return;

            var meshesToExport = _sceneManager.GetMeshes().ToList();
            var savedPath = _modelExporter.ExportToObj(meshesToExport, path);

            if (savedPath != null)
            {
                await ShowWpfMessageBoxAsync($"Exported all meshes to: {savedPath}", "Export Meshes", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                await ShowWpfMessageBoxAsync("Failed to export meshes.", "Export Meshes", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task ExportSingleMeshAsync(IAppMesh mesh)
        {
            if (mesh == null)
                return;

            string? path = PromptForSaveLocation();
            if (string.IsNullOrWhiteSpace(path))
                return;

            var exported = _modelExporter.ExportToObj(new List<IAppMesh> { mesh }, path);
            if (exported != null)
            {
                await ShowWpfMessageBoxAsync($"Exported mesh to: {exported}", "Export Mesh", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                await ShowWpfMessageBoxAsync("Failed to export mesh.", "Export Mesh", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string? PromptForSaveLocation()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Save Mesh As .obj",
                Filter = "Wavefront OBJ (*.obj)|*.obj",
                FileName = "export.obj"
            };

            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                return dialog.FileName;
            }
            return null;
        }

        private static async Task<MessageBoxResult> ShowWpfMessageBoxAsync(string message, string title, MessageBoxButton button, MessageBoxImage image)
        {
            return await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                System.Windows.Application.Current.MainWindow.Activate();
                return System.Windows.MessageBox.Show(
                    message,
                    title,
                    button,
                    image,
                    MessageBoxResult.OK,
                    System.Windows.MessageBoxOptions.DefaultDesktopOnly);
            });
        }
    }
}
