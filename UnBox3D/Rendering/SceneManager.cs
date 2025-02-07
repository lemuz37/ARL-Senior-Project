using OpenTK.Mathematics;
using g3;


namespace UnBox3D.Rendering
{

    public interface ISceneManager
    {
        List<IAppMesh> GetMeshes();
        void AddMeshes(List<IAppMesh> meshes);
        void AddMesh(IAppMesh mesh);
        void DeleteMesh(IAppMesh mesh);
        void RemoveSmallMeshes(float threshold);
    }
    public class SceneManager: ISceneManager
    {
        public List<IAppMesh> _sceneMeshes;

        public SceneManager()
        {
            _sceneMeshes = new List<IAppMesh>();
        }

        // Get the current list of meshes
        public List<IAppMesh> GetMeshes()
        {
            return new List<IAppMesh>(_sceneMeshes);
        }

        public void AddMeshes(List<IAppMesh> meshes)
        {
            _sceneMeshes.AddRange(meshes);
        }

        public void AddMesh(IAppMesh mesh) 
        {
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
            _sceneMeshes = _sceneMeshes
                .Where(mesh => GetMeshSize(mesh.GetG3Mesh()) >= threshold)
                .ToList();
        }

        private float GetMeshSize(DMesh3 mesh)
        {
            if (mesh.VertexCount == 0) return 0;

            Vector3 min = new Vector3(float.MaxValue);
            Vector3 max = new Vector3(float.MinValue);

            for (int i = 0; i < mesh.VertexCount; i++)
            {
                var vertex = mesh.GetVertex(i);
                min = Vector3.ComponentMin(min, new Vector3((float)vertex.x, (float)vertex.y, (float)vertex.z));
                max = Vector3.ComponentMax(max, new Vector3((float)vertex.x, (float)vertex.y, (float)vertex.z));
            }

            return (max - min).Length;
        }
    }
}
