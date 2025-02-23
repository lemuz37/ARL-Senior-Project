using Assimp;
using g3;
using OpenTK.Mathematics;
using UnBox3D.Rendering;
using UnBox3D.Utils;


namespace UnBox3D.Models
{
    public class ModelImporter
    {
        private readonly ISettingsManager _settingsManager;
        Vector3 defaultMeshColor;
        

        public ModelImporter(ISettingsManager settingsManager) 
        {
            _settingsManager = settingsManager;
        }

        public List<IAppMesh> ImportModel(string path)
        {
            Colors.colorMap.TryGetValue(_settingsManager.GetSetting<string>("RenderingSettings", "DefaultMeshColor"), out defaultMeshColor);

            List<IAppMesh> appMeshes = new List<IAppMesh>();

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

                    Scene scene = importer.ImportFile(path, postProcessFlags);

                    if (scene == null || scene.SceneFlags == SceneFlags.Incomplete || scene.RootNode == null)
                        return appMeshes;

                    foreach (var assimpMesh in scene.Meshes)
                    {
                        DMesh3 dmesh = new DMesh3();

                        // Add vertices to the DMesh3 object
                        foreach (var vertex in assimpMesh.Vertices)
                        {
                            dmesh.AppendVertex(new g3.Vector3d(vertex.X, vertex.Y, vertex.Z));
                        }

                        // Add triangles to the DMesh3 object
                        foreach (var face in assimpMesh.Faces)
                        {
                            if (face.IndexCount == 3)
                            {
                                dmesh.AppendTriangle(face.Indices[0], face.Indices[1], face.Indices[2]);
                            }
                        }

                        // Wrap the DMesh3 into an AppMesh
                        IAppMesh appMesh = new AppMesh(dmesh, assimpMesh);
                        appMesh.SetColor(defaultMeshColor);
                        appMesh.SetupMesh();
                        appMeshes.Add(appMesh);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading model: {ex.Message}");
            }

            return appMeshes;
        }
    }
}
