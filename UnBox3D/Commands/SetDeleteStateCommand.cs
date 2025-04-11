using g4;
using OpenTK.Mathematics;
using UnBox3D.Utils;
using UnBox3D.Models;
using UnBox3D.Controls;
using UnBox3D.Controls.States;
using UnBox3D.Rendering;
using UnBox3D.Rendering.OpenGL;

namespace UnBox3D.Commands
{
    class SetDeleteStateCommand : ICommand 
    {
        private IGLControlHost _glControlHost;
        private MouseController _mouseController;
        private ISceneManager _sceneManager;
        private ICamera _camera;
        private IRayCaster _rayCaster;
        private ICommandHistory _commandHistory;
        private IState _defaultState;

        public SetDeleteStateCommand(IGLControlHost glControlHost, MouseController mouseController, ISceneManager sceneManager, ICamera camera, IRayCaster rayCaster, ICommandHistory commandHistory)
        {
            _glControlHost = glControlHost;
            _mouseController = mouseController;
            _sceneManager = sceneManager;
            _camera = camera;
            _rayCaster = rayCaster;
            _commandHistory = commandHistory;
        }

        public void Execute() 
        {
            var deleteState = new DeleteState(_glControlHost, _sceneManager, _camera, new RayCaster(_glControlHost, _camera), _commandHistory);
            _mouseController.SetState(deleteState);
        }

        public void Undo()
        {
            _defaultState = new DefaultState(_sceneManager, _glControlHost, _camera, _rayCaster);
            _mouseController.SetState(_defaultState);
        }
    }
}
