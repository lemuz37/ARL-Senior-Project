using CommunityToolkit.Mvvm.ComponentModel;
using OpenTK.Mathematics;
using System.Collections.ObjectModel;
using UnBox3D.Rendering;
using UnBox3D.Utils;

namespace UnBox3D.ViewModels
{
    public partial class SceneEditingViewModel : ObservableObject
    {
        private readonly ISceneManager _sceneManager;
        private readonly ILogger _logger;

        public ObservableCollection<IAppMesh> Meshes { get; } = new();

        public SceneEditingViewModel(ISceneManager sceneManager, ILogger logger)
        {
            _sceneManager = sceneManager;
            _logger = logger;

            UpdateMeshesFromScene();
        }

        public void ClearScene()
        {
            _sceneManager.ClearScene();
            Meshes.Clear();
            _logger.Info("Scene cleared.");
        }

        public void DeleteMesh(IAppMesh mesh)
        {
            if (mesh == null)
                return;

            _sceneManager.DeleteMesh(mesh);
            if (mesh is IDisposable disposableMesh)
            {
                disposableMesh.Dispose();
            }

            Meshes.Remove(mesh);
            _logger.Info($"Deleted mesh: {mesh?.Name ?? "[null]"}");
        }

        public void ScaleSceneMeshes(float targetSize, Axis axis)
        {
            _sceneManager.ScaleAllMeshesToTargetDimension(targetSize, axis);
            Meshes.Clear();
            UpdateMeshesFromScene();
        }

        public void ReplaceSceneWithBoundingBoxes()
        {
            List<AppMesh> boundingBoxes = _sceneManager.LoadBoundingBoxes();

            _sceneManager.ClearScene();
            Meshes.Clear();

            foreach (var box in boundingBoxes)
            {
                _sceneManager.AddMesh(box);
                Meshes.Add(box);
            }

            _logger.Info($"Scene replaced with {boundingBoxes.Count} bounding box meshes.");
        }

        public void ReplaceMeshWithCube(IAppMesh mesh)
        {
            if (mesh == null)
                return;

            Vector3 center = _sceneManager.GetMeshCenter(mesh.GetG4Mesh());
            Vector3 dimensions = _sceneManager.GetMeshDimensions(mesh.GetG4Mesh());

            AppMesh cube = GeometryGenerator.CreateBox(
                center,
                dimensions.X,
                dimensions.Y,
                dimensions.Z,
                "Cube"
            );

            _sceneManager.ReplaceMesh(mesh, cube);

            UpdateMeshesFromScene();
            _logger.Info($"Replaced mesh '{mesh.Name}' with generated cube.");
        }


        public void ReplaceMeshWithCylinder(IAppMesh mesh)
        {
            if (mesh == null)
                return;

            Vector3 center = _sceneManager.GetMeshCenter(mesh.GetG4Mesh());
            Vector3 dimensions = _sceneManager.GetMeshDimensions(mesh.GetG4Mesh());

            bool isXAligned = dimensions.X < dimensions.Z;
            float radius = Math.Max(Math.Min(dimensions.X, dimensions.Z), dimensions.Y) / 2;
            float height = isXAligned ? dimensions.X : dimensions.Z;

            AppMesh cylinder = GeometryGenerator.CreateCylinder(center, radius, height, 32);

            _sceneManager.ReplaceMesh(mesh, cylinder);

            UpdateMeshesFromScene();
            _logger.Info($"Replaced mesh '{mesh.Name}' with generated cylinder.");
        }

        public void RemoveSmallMeshes(float thresholdPercentage)
        {
            float thresholdRatio = thresholdPercentage / 100f;

            _sceneManager.RemoveSmallMeshes(thresholdRatio);
            UpdateMeshesFromScene();

            _logger.Info($"Removed meshes smaller than {thresholdPercentage}% threshold.");
        }

        public void UpdateMeshesFromScene()
        {
            Meshes.Clear();
            foreach (var mesh in _sceneManager.GetMeshes())
            {
                Meshes.Add(mesh);
            }
        }
    }
}
