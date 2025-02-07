using g3;
using OpenTK.Mathematics;
using UnBox3D.Utils;
using UnBox3D.Models;
using UnBox3D.Commands;
using UnBox3D.Rendering;
using UnBox3D.Rendering.OpenGL;

namespace UnBox3D.Controls.States
{
    // InteractionHandler to handle model interaction logic
    public class DeleteState : IState
    {
        private readonly ISceneManager _sceneManager;
        private readonly IGLControlHost _glControlHost;
        private readonly IRayCaster _rayCaster;
        private readonly ICamera _camera;
        private ICommandHistory _commandHistory;


        public DeleteState(IGLControlHost glControlHost, ISceneManager sceneManager, ICamera camera, IRayCaster rayCaster, ICommandHistory commandHistory)
        {
            _glControlHost = glControlHost;
            _sceneManager = sceneManager;
            _camera = camera;
            _rayCaster = rayCaster;
            _commandHistory = commandHistory;
        }

        public void OnMouseDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            ICommand deleteCommand = new DeleteCommand(_glControlHost, _sceneManager, _rayCaster, _camera);
            _commandHistory.PushCommand(deleteCommand);
            deleteCommand.Execute();
        }

        public void OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {

        }

        public void OnMouseUp(System.Windows.Input.MouseButtonEventArgs e)
        {

        }
    }
}
