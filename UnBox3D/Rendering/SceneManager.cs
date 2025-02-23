using OpenTK.Mathematics;
using g3;
using System.Collections.ObjectModel;


namespace UnBox3D.Rendering
{

    public interface ISceneManager
    {
        ObservableCollection<IAppMesh> GetMeshes();
        void AddMesh(IAppMesh mesh);
        void DeleteMesh(IAppMesh mesh);
        void RemoveSmallMeshes(float threshold);
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
