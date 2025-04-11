using g4;
using OpenTK.Mathematics;
using UnBox3D.Rendering;

namespace UnBox3D.Commands
{
    public class MoveCommand : ICommand
    {
        private IAppMesh _mesh;
        private Vector3 _movement;
        private readonly Stack<MeshMoveMemento> movedMeshes;

        public MoveCommand(IAppMesh mesh, Vector3 movement)
        {
            _mesh = mesh;
            _movement = movement;
            movedMeshes = new Stack<MeshMoveMemento>();
        }

        public void Execute()
        {
            //_mesh.Move(_movement);
            MeshMoveMemento movedMesh = new MeshMoveMemento(_mesh, _movement);
            movedMeshes.Push(movedMesh);
        }

        public void Undo()
        {
            if (movedMeshes.Count > 0)
            {
                MeshMoveMemento movedMeshMemento = movedMeshes.Pop();
                IAppMesh mesh = movedMeshMemento.Mesh;
                Vector3 movement = movedMeshMemento.Movement;
                //mesh.Move(-movement);
            }
        }
    }

    public class MeshMoveMemento
    {
        public IAppMesh Mesh { get; }
        public Vector3 Movement { get; }

        public MeshMoveMemento(IAppMesh mesh, Vector3 movement)
        {
            Mesh = mesh;
            Movement = movement;
        }
    }
}
