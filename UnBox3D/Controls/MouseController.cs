using System;
using System.Windows;
using System.Windows.Input; // For WPF input handling
using OpenTK.Mathematics;
using UnBox3D.Rendering;
using UnBox3D.Controls.States;
using UnBox3D.Utils;

namespace UnBox3D.Controls
{
    public class MouseController
    {
        private bool _isPanning;
        private bool _isYawingAndPitching;
        private System.Windows.Point _lastMousePosition;

        private readonly ISettingsManager _settingsManager;
        private readonly ICamera _camera;
        private IState _currentState;

        private readonly float _cameraYawSensitivity;
        private readonly float _cameraPitchSensitivity;
        private readonly float _cameraPanSensitivity;
        private readonly float _zoomSensitivity;

        public MouseController(ISettingsManager settingsManager, ICamera camera, IState neutralState)
        {
            _settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));
            _camera = camera ?? throw new ArgumentNullException(nameof(camera));
            _currentState = neutralState ?? throw new ArgumentNullException(nameof(neutralState));

            // Apply settings
            _cameraYawSensitivity = _settingsManager.GetSetting<float>(new UISettings().GetKey(), UISettings.CameraYawSensitivity);
            _cameraPitchSensitivity = _settingsManager.GetSetting<float>(new UISettings().GetKey(), UISettings.CameraPitchSensitivity);
            _cameraPanSensitivity = _settingsManager.GetSetting<float>(new UISettings().GetKey(), UISettings.CameraPanSensitivity);
            _zoomSensitivity = _settingsManager.GetSetting<float>(new UISettings().GetKey(), UISettings.ZoomSensitivity);

            // Attach mouse event handlers to the main WPF window
            var mainWindow = System.Windows.Application.Current.MainWindow;
            if (mainWindow != null)
            {
                mainWindow.MouseDown += OnMouseDown;
                mainWindow.MouseMove += OnMouseMove;
                mainWindow.MouseUp += OnMouseUp;
                mainWindow.MouseWheel += OnMouseWheel;
            }
        }

        public void SetState(IState newState) => _currentState = newState;
        public IState GetState() => _currentState;

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            _lastMousePosition = e.GetPosition(System.Windows.Application.Current.MainWindow);

            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    _currentState?.OnMouseDown(e);
                    break;
                case MouseButton.Right:
                    _isPanning = true;
                    break;
                case MouseButton.Middle:
                    _isYawingAndPitching = true;
                    break;
            }
        }

        private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var currentMousePosition = e.GetPosition(System.Windows.Application.Current.MainWindow);
            Vector2 delta = CalculateMouseDelta(currentMousePosition);

            if (_isPanning)
            {
                PanCamera(delta);
            }
            else if (_isYawingAndPitching)
            {
                AdjustCameraYawAndPitch(delta);
            }
            else
            {
                _currentState?.OnMouseMove(e);
            }

            _lastMousePosition = currentMousePosition;
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    _currentState?.OnMouseUp(e);
                    break;
                case MouseButton.Right:
                    _isPanning = false;
                    break;
                case MouseButton.Middle:
                    _isYawingAndPitching = false;
                    break;
            }
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            AdjustCameraZoom(e.Delta / 120.0f); // Normalize wheel delta
        }

        private void PanCamera(Vector2 delta)
        {
            float deltaX = delta.X / (float)System.Windows.Application.Current.MainWindow.Width;
            float deltaY = delta.Y / (float)System.Windows.Application.Current.MainWindow.Height;

            _camera.Position -= _camera.Right * deltaX * _cameraPanSensitivity;
            _camera.Position += _camera.Up * deltaY * _cameraPanSensitivity;
        }

        private void AdjustCameraYawAndPitch(Vector2 delta)
        {
            _camera.Yaw += delta.X * _cameraYawSensitivity;
            _camera.Pitch -= delta.Y * _cameraPitchSensitivity;
        }

        private void AdjustCameraZoom(float delta)
        {
            _camera.Position += _camera.Front * delta * _zoomSensitivity;
        }

        private Vector2 CalculateMouseDelta(System.Windows.Point currentMousePosition)
        {
            float deltaX = (float)(currentMousePosition.X - _lastMousePosition.X);
            float deltaY = (float)(currentMousePosition.Y - _lastMousePosition.Y);
            return new Vector2(deltaX, deltaY);
        }
    }
}
