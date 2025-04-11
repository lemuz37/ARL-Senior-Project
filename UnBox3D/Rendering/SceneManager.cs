using OpenTK.Mathematics;
using g3;
using System.Collections.ObjectModel;
using UnBox3D.Utils;


namespace UnBox3D.Rendering
{

    public interface ISceneManager
    {
        ObservableCollection<IAppMesh> GetMeshes();
        void AddMesh(IAppMesh mesh);
        void DeleteMesh(IAppMesh mesh);
        void RemoveSmallMeshes(float threshold);
        Vector3 GetMeshCenter(DMesh3 mesh);
        Vector3 GetMeshDimensions(DMesh3 mesh);
        //List<DMesh3> LoadRotatedCylinder(Vector3 center, float radius, float height, int segments, Vector3 direction);
        List<AppMesh> LoadBoundingBoxes();
    }
    public class SceneManager: ISceneManager
    {
        private ObservableCollection<IAppMesh> _sceneMeshes = new();

        public ObservableCollection<IAppMesh> GetMeshes() => _sceneMeshes;

        public SceneManager()
        {
            _sceneMeshes = new();
        }

        public void AddMesh(IAppMesh mesh) 
        {
            if (mesh != null)
                _sceneMeshes.Add(mesh);
        }
        /*
        public void RotateMesh(DMesh3 mesh, Matrix4 rotationMatrix)
        {
            if (mesh != null)
            {
                for (int i = 0; i < mesh.VertexCount; i++)
                {
                    // Retrieve the vertex
                    g3.Vector3d vertex = mesh.GetVertex(i);

                    // Convert the vertex to a Vector4 for transformation (homogeneous coordinates)
                    Vector4 transformedVertex = new Vector4((float)vertex.x, (float)vertex.y, (float)vertex.z, 1.0f);

                    // Apply the rotation matrix
                    transformedVertex = Vector4.Transform(transformedVertex, rotationMatrix);

                    // Update the vertex with the transformed coordinates
                    mesh.SetVertex(i, new g3.Vector3d(transformedVertex.X, transformedVertex.Y, transformedVertex.Z));
                }
            }
        }*/


        public void RemoveMeshes() 
        {

        }

        public void DeleteMesh(IAppMesh mesh) 
        {
            _sceneMeshes.Remove(mesh);
        }

        public void RemoveSmallMeshes(float threshold)
        {
            // Use a list to track meshes to remove
            var meshesToRemove = _sceneMeshes
                .Where(mesh => GetMeshSize(mesh.GetG3Mesh()) < threshold)
                .ToList();

            // Remove each mesh individually (triggers UI update)
            foreach (var mesh in meshesToRemove)
            {
                _sceneMeshes.Remove(mesh);
            }
        }

        public Vector3 GetMeshCenter(DMesh3 mesh)
        {
            if (mesh.VertexCount > 0)
            {
                Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

                for (int i = 0; i < mesh.VertexCount; i++)
                {
                    g3.Vector3d vertex = mesh.GetVertex(i);
                    Vector3 vertexVec = new Vector3((float)vertex.x, (float)vertex.y, (float)vertex.z);

                    // Update min and max
                    min = Vector3.ComponentMin(min, vertexVec);
                    max = Vector3.ComponentMax(max, vertexVec);
                }

                // Calculate the center
                return (min + max) * 0.5f;
            }

            return Vector3.Zero;
        }

        public Vector3 GetMeshDimensions(DMesh3 mesh)
        {
            if (mesh.VertexCount > 0)
            {
                float largestXDimension = 0.0f;
                float largestYDimension = 0.0f;
                float largestZDimension = 0.0f;

                Vector3 meshCenter = GetMeshCenter(mesh);

                for (int i = 0; i < mesh.VertexCount; i++)
                {
                    g3.Vector3d vertex = mesh.GetVertex(i);
                    Vector3 vertexVec = new Vector3((float)vertex.x, (float)vertex.y, (float)vertex.z);

                    if (largestXDimension < (vertexVec.X - meshCenter.X))
                    {
                        largestXDimension = vertexVec.X - meshCenter.X;
                    }
                    if (largestYDimension < (vertexVec.Y - meshCenter.Y))
                    {
                        largestYDimension = vertexVec.Y - meshCenter.Y;
                    }
                    if (largestZDimension < (vertexVec.Z - meshCenter.Z))
                    {
                        largestZDimension = vertexVec.Z - meshCenter.Z;
                    }
                }

                return new Vector3(largestXDimension * 2, largestYDimension * 2, largestZDimension * 2); // multiplying by 2 because thats the largest distance from the center
            }

            return Vector3.Zero;
        }

        public List<AppMesh> LoadBoundingBoxes()
        {
            var originalMeshes = _sceneMeshes.ToList();
            var boxMeshes = new List<AppMesh>();

            foreach (IAppMesh mesh in originalMeshes)
            {
                DMesh3 geomMesh = mesh.GetG3Mesh();
                Vector3 meshCenter = GetMeshCenter(geomMesh);
                Vector3 meshDimensions = GetMeshDimensions(geomMesh);

                AppMesh boxMesh = GeometryGenerator.CreateBox(meshCenter, meshDimensions.X, meshDimensions.Y, meshDimensions.Z, mesh.Name);
                boxMeshes.Add(boxMesh);

                _sceneMeshes.Add(boxMesh);
                _sceneMeshes.Remove(mesh);
            }

            return boxMeshes;
        }

        private float GetMeshSize(DMesh3 mesh)
        {
            if (mesh.VertexCount == 0) return 0;

            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            foreach (int i in mesh.VertexIndices())
            {
                var vertex = mesh.GetVertex(i);
                Vector3 vertexVec = new Vector3((float)vertex.x, (float)vertex.y, (float)vertex.z);

                min = Vector3.ComponentMin(min, vertexVec);
                max = Vector3.ComponentMax(max, vertexVec);
            }

            return (max - min).Length;
        }

    }
}
