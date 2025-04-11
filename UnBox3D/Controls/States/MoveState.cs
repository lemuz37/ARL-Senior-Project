using g4;
using OpenTK.Mathematics;
using UnBox3D.Utils;
using UnBox3D.Models;
using UnBox3D.Commands;
using UnBox3D.Rendering;
using UnBox3D.Rendering.OpenGL;

namespace UnBox3D.Controls.States
{
    public class MoveState : IState
    {
        private readonly IGLControlHost _controlHost;
        private readonly ISceneManager _sceneManager;
        private readonly ICamera _camera;
        private readonly IRayCaster _rayCaster;
        private readonly ICommandHistory _commandHistory;

        private bool _isDragging;
        private Point _lastMousePosition;
        private Vector3 _accumulatedMovement;
        private IAppMesh? _selectedMesh;
        private IAppMesh? _lastClickedMesh;
        private readonly float _meshMoveSensitivity;

        public MoveState(
            IGLControlHost controlHost,
            ISceneManager sceneManager,
            ICamera camera,
            IRayCaster rayCaster,
            ICommandHistory commandHistory,
            float meshMoveSensitivity = 1.0f)
        {
            _controlHost = controlHost ?? throw new ArgumentNullException(nameof(controlHost));
            _sceneManager = sceneManager ?? throw new ArgumentNullException(nameof(sceneManager));
            _camera = camera ?? throw new ArgumentNullException(nameof(camera));
            _rayCaster = rayCaster ?? throw new ArgumentNullException(nameof(rayCaster));
            _commandHistory = commandHistory ?? throw new ArgumentNullException(nameof(commandHistory));
            _meshMoveSensitivity = meshMoveSensitivity;
        }

        public void OnMouseDown(MouseEventArgs e)
        {
            // Record mouse position
            _lastMousePosition = Control.MousePosition;

            // Perform raycasting to select a mesh
            Vector3 rayOrigin = _camera.Position;
            Vector3 rayDirection = _rayCaster.GetRay();

            if (_rayCaster.RayIntersectsMesh(_sceneManager.GetMeshes(), rayOrigin, rayDirection, out float _, out IAppMesh clickedMesh))
            {
                _selectedMesh = clickedMesh;
                _isDragging = true;
            }
        }

        public void OnMouseMove(MouseEventArgs e)
        {
            if (!_isDragging || _selectedMesh == null)
                return;

            // Calculate movement delta
            var currentMousePosition = Control.MousePosition;
            float deltaX = (currentMousePosition.X - _lastMousePosition.X) * _meshMoveSensitivity;
            float deltaY = (currentMousePosition.Y - _lastMousePosition.Y) * _meshMoveSensitivity;

            // Accumulate movement
            _accumulatedMovement += new Vector3(deltaX, 0, deltaY);

            // Redraw the scene for visual feedback
            _controlHost.Invalidate();

            // Update last mouse position
            _lastMousePosition = currentMousePosition;
        }

        public void OnMouseUp(MouseEventArgs e)
        {
            _lastClickedMesh = _selectedMesh;

            if (_accumulatedMovement != Vector3.Zero && _selectedMesh != null)
            {
                // Create and execute move command
                ICommand moveCommand = new MoveCommand(_selectedMesh, _accumulatedMovement);
                _accumulatedMovement = Vector3.Zero;

                _commandHistory.PushCommand(moveCommand);
                moveCommand.Execute();
            }

            // Reset dragging state
            _isDragging = false;
        }
    }
}
