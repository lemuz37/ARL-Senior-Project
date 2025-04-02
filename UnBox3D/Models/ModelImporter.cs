using Assimp;
using g3;
using System.Diagnostics;
using UnBox3D.Rendering;
using UnBox3D.Utils;


namespace UnBox3D.Models
{
    public class ModelImporter
    {
        private readonly ISettingsManager _settingsManager;
        private List<IAppMesh> importedMeshes;
        private Scene scene;
        public ModelImporter(ISettingsManager settingsManager) 
        {
            _settingsManager = settingsManager;
        }

        public List<IAppMesh> ImportModel(string path)
        {
            try
            {
                using (var importer = new AssimpContext())
                {
                    var postProcessFlags = PostProcessSteps.Triangulate |
                                           PostProcessSteps.JoinIdenticalVertices |
                                           PostProcessSteps.RemoveComponent |
                                           PostProcessSteps.SplitLargeMeshes |
                                           PostProcessSteps.OptimizeMeshes |
                                           PostProcessSteps.FindDegenerates |
                                           PostProcessSteps.FindInvalidData;

                    scene = importer.ImportFile(path, postProcessFlags);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading model: {ex.Message}");
            }

            importedMeshes = new List<IAppMesh>();

            for (int i = 0; i < scene.MeshCount; i++)
            {
                Mesh mesh = scene.Meshes[i];
                DMesh3 dmesh = new DMesh3();

                // Add vertices to the DMesh3 object
                for (int j = 0; j < mesh.VertexCount; j++)
                {
                    Vector3D vertex = mesh.Vertices[j];
                    dmesh.AppendVertex(new g3.Vector3d(vertex.X, vertex.Y, vertex.Z));
                }

                // Add triangles to the DMesh3 object
                for (int j = 0; j < mesh.FaceCount; j++) 
                {
                    Face face = mesh.Faces[j];

                    if (face.HasIndices && face.IndexCount == 3) 
                    {
                        dmesh.AppendTriangle(face.Indices[0], face.Indices[1], face.Indices[2]);
                    }
                }

                IAppMesh appMesh = new AppMesh(dmesh, mesh);
                //_settingsManager.
                importedMeshes.Add(appMesh);
            }
            return importedMeshes;
        }
    }
}
