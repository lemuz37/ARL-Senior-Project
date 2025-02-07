using UnBox3D.Controls;
using UnBox3D.Controls.States;
using UnBox3D.Models;
using UnBox3D.Rendering;
using UnBox3D.Rendering.OpenGL;
using UnBox3D.Utils;

namespace UnBox3D.Commands
{
    class SetRotateStateCommand : ICommand 
    {
        private readonly ISettingsManager _settingsManager;
        private readonly IGLControlHost _controlHost;
        private readonly ISceneManager _sceneManager;
        private readonly ICamera _camera;
        private readonly IRayCaster _rayCaster;
        private MouseController mouseController;
        private ICommandHistory _commandHistory;
        private IState? _defaultState;

        public SetRotateStateCommand(ISettingsManager settingsManager, IGLControlHost controlHost, ISceneManager sceneManager, ICamera camera, IRayCaster rayCaster, ICommandHistory commandHistory)
        {
            _settingsManager = settingsManager;
            _controlHost = controlHost;
            _sceneManager = sceneManager;
            _camera = camera;
            _rayCaster = rayCaster;
            _commandHistory = commandHistory;
        }

        public void Execute() 
        {
            var rotateState = new RotateState(_settingsManager, _sceneManager, _controlHost, _camera, _rayCaster, _commandHistory);
            mouseController.SetState(rotateState);
        }

        public void Undo()
        {
            _defaultState = new DefaultState(_sceneManager, _controlHost, _camera, _rayCaster);
            mouseController.SetState(_defaultState);
        }
    }
}
