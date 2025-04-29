using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using UnBox3D.Models;
using UnBox3D.Utils;
using UnBox3D.Rendering;
using System.IO;
using System.Windows;

namespace UnBox3D.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        #region Fields & Properties

        private readonly ISettingsManager _settingsManager;
        private readonly ILogger _logger;
        private readonly IFileSystem _fileSystem;
        private readonly ISceneManager _sceneManager;
        private readonly ModelExporter _modelExporter;
        private readonly ICommandHistory _commandHistory;
        public ImportExportViewModel ImportExport { get; }
        public SimplificationViewModel Simplification { get; }
        public SceneEditingViewModel SceneEditing { get; }
        public UnfoldingViewModel Unfolding { get; }

        [ObservableProperty]
        private IAppMesh selectedMesh;

        [ObservableProperty]
        private float pageWidth = 25.0f;

        [ObservableProperty]
        private float pageHeight = 25.0f;

        [ObservableProperty]
        private float simplificationRatio = 50f;

        [ObservableProperty]
        private float smallMeshThreshold = 0f;

        public ObservableCollection<MeshSummary> Meshes { get; } = new();
        #endregion

        #region Constructor

        public MainViewModel(ILogger logger, ISettingsManager settingsManager, ISceneManager sceneManager,
                             IFileSystem fileSystem, BlenderIntegration blenderIntegration, IBlenderInstaller blenderInstaller,
                             ModelExporter modelExporter, ICommandHistory commandHistory, SVGEditor svgEditor)
        {
            _logger = logger;
            _settingsManager = settingsManager;
            _sceneManager = sceneManager;
            _fileSystem = fileSystem;
            _modelExporter = modelExporter;
            _commandHistory = commandHistory;

            ImportExport = new ImportExportViewModel( settingsManager, logger, fileSystem, sceneManager, modelExporter);
            Simplification = new SimplificationViewModel(settingsManager, logger, fileSystem, sceneManager, modelExporter);
            SceneEditing = new SceneEditingViewModel(sceneManager, logger);
            Unfolding = new UnfoldingViewModel(settingsManager, logger, fileSystem, blenderInstaller, blenderIntegration,  svgEditor);
        }

        #endregion

        #region Model Import/Export Methods

        [RelayCommand]
        private void ImportObjModel()
        {
            ImportExport.ImportObjModel();
            UpdateMeshesFromScene();
        }

        [RelayCommand]
        private async Task ExportScene()
        {
            await ImportExport.ExportAllMeshesAsync();
        }

        [RelayCommand]
        private async Task ExportMesh(IAppMesh mesh)
        {
            await ImportExport.ExportSingleMeshAsync(mesh);
        }
        #endregion

        #region Unfolding Methods

        [RelayCommand]
        private async Task ExportAndUnfoldAllMeshes()
        {
            var meshes = _sceneManager.GetMeshes().ToList();
            if (meshes.Count == 0)
            {
                await ShowWpfMessageBoxAsync("There are no meshes in the scene to unfold.", "Unfolding Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string fileName = $"unfold_scene_{Guid.NewGuid()}.obj";
            string? exportedPath = _modelExporter.ExportToObj(meshes, fileName);

            if (string.IsNullOrWhiteSpace(exportedPath))
            {
                await ShowWpfMessageBoxAsync("Failed to export scene for unfolding.", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                bool success = await Unfolding.ProcessUnfoldingAsync(exportedPath, PageWidth, PageHeight);

                if (success)
                {
                    await ShowWpfMessageBoxAsync("Scene unfolded successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    await ShowWpfMessageBoxAsync("Unfolding failed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (TimeoutException)
            {
                await ShowWpfMessageBoxAsync("Unfolding operation timed out. Try simplifying your model.", "Timeout", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                await ShowWpfMessageBoxAsync($"Unfolding failed.\n\nDetails:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task UnfoldMesh(IAppMesh mesh)
        {
            if (mesh == null)
                return;

            string fileName = $"unfold_single_{Guid.NewGuid()}.obj";
            string? exportedPath = _modelExporter.ExportToObj(new List<IAppMesh> { mesh }, fileName);

            if (string.IsNullOrWhiteSpace(exportedPath))
            {
                await ShowWpfMessageBoxAsync("Failed to export mesh for unfolding.", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                bool success = await Unfolding.ProcessUnfoldingAsync(exportedPath, PageWidth, PageHeight);

                if (success)
                {
                    await ShowWpfMessageBoxAsync("Mesh unfolded successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    await ShowWpfMessageBoxAsync("Unfolding failed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (TimeoutException)
            {
                await ShowWpfMessageBoxAsync("Unfolding operation timed out. Try simplifying your model.", "Timeout", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                await ShowWpfMessageBoxAsync($"Unfolding failed.\n\nDetails:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Scene Editing Commands

        [RelayCommand]
        private void ReplaceSceneWithBoundingBoxes()
        {
            SceneEditing.ReplaceSceneWithBoundingBoxes();
            UpdateMeshesFromScene();
        }

        [RelayCommand]
        private void RemoveSmallMeshes()
        {
            SceneEditing.RemoveSmallMeshes(SmallMeshThreshold);
            UpdateMeshesFromScene();
        }

        [RelayCommand]
        private void ReplaceMeshWithCube(IAppMesh mesh)
        {
            if (mesh == null)
                return;
            SceneEditing.ReplaceMeshWithCube(mesh);
            UpdateMeshesFromScene();
        }

        [RelayCommand]
        private void ReplaceMeshWithCylinder(IAppMesh mesh)
        {
            if (mesh == null)
                return;

            SceneEditing.ReplaceMeshWithCylinder(mesh);
            UpdateMeshesFromScene();
        }

        [RelayCommand]
        public void ResizeAllMeshes((float targetSize, Axis axis) scaleParams)
        {
            SceneEditing.ScaleSceneMeshes(scaleParams.targetSize, scaleParams.axis);
            UpdateMeshesFromScene();
        }

        [RelayCommand]
        private void DeleteMesh(IAppMesh mesh)
        {
            SceneEditing.DeleteMesh(mesh);
            UpdateMeshesFromScene();
        }

        [RelayCommand]
        private async Task ClearScene()
        {
            var result = await ShowWpfMessageBoxAsync("Are you sure you want to clear the scene?",
                                                      "Clear Scene",
                                                      MessageBoxButton.YesNo,
                                                      MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                SceneEditing.ClearScene();
                UpdateMeshesFromScene();
            }
        }
        #endregion

        #region Simplification Commands
        [RelayCommand]
        private async Task SimplifyMeshQEC(IAppMesh mesh)
        {
            if (mesh == null) return;

            bool success = await Simplification.SimplifyMeshAsync(mesh, "quadric_edge_collapse", SimplificationRatio);
            if (success)
                UpdateMeshesFromScene();
            else
                await ShowWpfMessageBoxAsync("Simplifying mesh with QEC failed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        [RelayCommand]
        private async Task SimplifyMeshFQD(IAppMesh mesh)
        {
            if (mesh == null) return;

            bool success = await Simplification.SimplifyMeshAsync(mesh, "fast_quadric_decimation", SimplificationRatio);
            if (success)
                UpdateMeshesFromScene();
            else
                await ShowWpfMessageBoxAsync("Simplifying mesh with FQD failed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        [RelayCommand]
        private async Task SimplifyMeshVC(IAppMesh mesh)
        {
            if (mesh == null) return;

            bool success = await Simplification.SimplifyMeshAsync(mesh, "vertex_clustering", SimplificationRatio);
            if (success)
                UpdateMeshesFromScene();
            else
                await ShowWpfMessageBoxAsync("Simplifying mesh with VC failed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        [RelayCommand]
        private async Task SimplifyAllQEC()
        {
            bool success = await Simplification.SimplifyAllAsync("quadric_edge_collapse", SimplificationRatio);
            if (success)
                UpdateMeshesFromScene();
            else
                await ShowWpfMessageBoxAsync("Simplification failed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }


        [RelayCommand]
        private async Task SimplifyAllFQD()
        {
            bool success = await Simplification.SimplifyAllAsync("fast_quadric_decimation", SimplificationRatio);
            if (success)
                UpdateMeshesFromScene();
            else
                await ShowWpfMessageBoxAsync("Simplification failed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        [RelayCommand]
        private async Task SimplifyAllVC()
        {
            bool success = await Simplification.SimplifyAllAsync("vertex_clustering", SimplificationRatio);
            if (success)
                UpdateMeshesFromScene();
            else
                await ShowWpfMessageBoxAsync("Simplification failed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Shows a WPF MessageBox asynchronously using the UI Dispatcher.
        /// </summary>
        private static async Task<MessageBoxResult> ShowWpfMessageBoxAsync(string message, string title, MessageBoxButton button, MessageBoxImage image)
        {
            return await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                // Activate main window so the MessageBox appears on top.
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

        public void UpdateMeshesFromScene()
        {
            Meshes.Clear();
            foreach (var mesh in _sceneManager.GetMeshes())
            {
                Meshes.Add(new MeshSummary(mesh));
            }
        }
        #endregion

        [RelayCommand]
        private void Exit()
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
