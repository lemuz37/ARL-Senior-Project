using Assimp;
using g4;
using System.Diagnostics;
using UnBox3D.Rendering;
using UnBox3D.Utils;
using OpenTK.Mathematics;

namespace UnBox3D.Models
{
    /// <summary>
    /// Handles importing 3D model files (such as .obj) using Assimp, applies post-processing, scaling, centering, and wraps them
    /// into IAppMesh objects for use in the rendering system.
    /// </summary>
    public class ModelImporter
    {
        private readonly ISettingsManager _settingsManager;
        private List<IAppMesh>? importedMeshes;
        private Scene? scene;
        private bool _wasScaled = false;
        public bool WasScaled => _wasScaled;


        public ModelImporter(ISettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
        }

        public List<IAppMesh> ImportModel(string path)
        {
            try
            {
                // Load the model using Assimp with various post-processing steps to clean and optimize the mesh
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
                return new List<IAppMesh>();
            }

            importedMeshes = new List<IAppMesh>();

            // Load default mesh color from settings, fallback to grey if not found or invalid
            string? colorName = _settingsManager.GetSetting<string>(
                new RenderingSettings().GetKey(),
                RenderingSettings.MeshColor
            );

            Vector3 defaultColor = Colors.Grey;
            if (!string.IsNullOrWhiteSpace(colorName))
            {
                string key = colorName.ToLower().Replace(" ", "");
                if (Colors.TryGetColor(key, out var parsedColor))
                {
                    defaultColor = parsedColor;
                }
                else
                {
                    Debug.WriteLine($"Warning: Mesh color '{colorName}' not recognized. Using grey.");
                }
            }

            AxisAlignedBox3d modelBounds = AxisAlignedBox3d.Empty;
            List<(DMesh3 dmesh, Mesh assimpMesh)> meshPairs = new();

            for (int i = 0; i < scene.MeshCount; i++)
            {
                Mesh mesh = scene.Meshes[i];
                DMesh3 dmesh = new DMesh3();

                // Copy vertices from Assimp mesh to g4 mesh
                for (int j = 0; j < mesh.VertexCount; j++)
                {
                    Vector3D v = mesh.Vertices[j];
                    dmesh.AppendVertex(new g4.Vector3d(v.X, v.Y, v.Z));
                }

                // Copy triangle faces from Assimp to g4
                for (int j = 0; j < mesh.FaceCount; j++)
                {
                    Face face = mesh.Faces[j];
                    if (face.HasIndices && face.IndexCount == 3)
                    {
                        dmesh.AppendTriangle(face.Indices[0], face.Indices[1], face.Indices[2]);
                    }
                }

                meshPairs.Add((dmesh, mesh));
            }

            foreach (var (dmesh, assimpMesh) in meshPairs)
            {
                IAppMesh appMesh = new AppMesh(dmesh, assimpMesh);
                appMesh.SetColor(defaultColor);
                importedMeshes.Add(appMesh);
            }

            return importedMeshes;
        }
    }
}
