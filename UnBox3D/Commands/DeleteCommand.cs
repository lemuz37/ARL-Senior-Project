using g4;
using OpenTK.Mathematics;
using UnBox3D.Rendering;
using UnBox3D.Rendering.OpenGL;
using UnBox3D.Utils;


namespace UnBox3D.Commands
{    public class DeleteCommand : ICommand
    {
        private readonly IGLControlHost _glControlHost;
        private readonly ISceneManager _sceneManager;
        private readonly ILogger _logger;
        private readonly IRayCaster _rayCaster;
        private readonly ICamera _camera;
        private readonly Stack<MeshDeleteMemento> deletedMeshes;

        public DeleteCommand(IGLControlHost glControlHost, ISceneManager sceneManager, IRayCaster rayCaster, ICamera camera)
        {
            _glControlHost = glControlHost;
            _sceneManager = sceneManager;
            this._rayCaster = rayCaster;
            _camera = camera;

            deletedMeshes = new Stack<MeshDeleteMemento>();
        }

        public void Execute()
        {
            // Get the world space ray from the MousePicker
            Vector3 rayWorld = _rayCaster.GetRay();
            Vector3 rayOrigin = _camera.Position;

            // Check for intersection with the model
            if (_rayCaster.RayIntersectsMesh(_sceneManager.GetMeshes(), rayOrigin, rayWorld, out float distance, out IAppMesh clickedMesh))
            {
                // Set the color for the current mesh
                MeshDeleteMemento deletedMesh = new MeshDeleteMemento(clickedMesh);

                // Push mesh memento to deleted meshes stack
                deletedMeshes.Push(deletedMesh);

                _sceneManager.DeleteMesh(clickedMesh);

                _glControlHost.Invalidate(); // Re-render
            }
        }

        public void Undo()
        {
            if (deletedMeshes.Count > 0)
            {
                MeshDeleteMemento deletedMeshMemento = deletedMeshes.Pop();
                IAppMesh MeshToRestore = deletedMeshMemento.Mesh;
                Vector3 color = deletedMeshMemento.Color;
                _logger.Info($"Restoring deleted mesh.");
                _sceneManager.AddMesh(MeshToRestore);
            }
            else
            {
                _logger.Warn("No deleted meshes to undo.");
            }
        }
    }

    public class MeshDeleteMemento 
    {
        public IAppMesh Mesh { get; }
        public Vector3 Color { get; }

        public MeshDeleteMemento(IAppMesh mesh)
        {
            Mesh = mesh;
        }
    }
}
