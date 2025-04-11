using Assimp;
using g4;
using OpenTK.Mathematics;
using UnBox3D.Rendering;
using System;
using System.Collections.Generic;
using Quaternion = OpenTK.Mathematics.Quaternion;

namespace UnBox3D.Commands
{
    public class RotateCommand : ICommand
    {
        private readonly IAppMesh _mesh;
        private readonly Quaternion _rotation;
        private readonly Stack<MeshRotateMemento> _rotatedMeshes;

        public RotateCommand(IAppMesh mesh, Quaternion rotation, float angle)
        {
            _mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));

            // Store the rotation quaternion
            _rotation = Quaternion.FromAxisAngle(rotation.Xyz.Normalized(), MathHelper.DegreesToRadians(angle));
            _rotatedMeshes = new Stack<MeshRotateMemento>();
        }

        public void Execute()
        {
            _mesh.Rotate(_rotation);

            // Store the undo rotation matrix
            Quaternion undoRotationMatrix = _rotation.Inverted();
            _rotatedMeshes.Push(new MeshRotateMemento(_mesh, undoRotationMatrix));
        }

        public void Undo()
        {
            if (_rotatedMeshes.Count > 0)
            {
                // Retrieve the last rotation operation
                MeshRotateMemento rotatedMeshMemento = _rotatedMeshes.Pop();

                // Undo the rotation
                rotatedMeshMemento.Mesh.Rotate(rotatedMeshMemento.UndoRotationMatrix);
            }
            else
            {
                throw new InvalidOperationException("No rotations to undo.");
            }
        }
    }

    public class MeshRotateMemento
    {
        public IAppMesh Mesh { get; }
        public Quaternion UndoRotationMatrix { get; }

        public MeshRotateMemento(IAppMesh mesh, Quaternion undoRotationMatrix)
        {
            Mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));
            UndoRotationMatrix = undoRotationMatrix;
        }
    }
}
