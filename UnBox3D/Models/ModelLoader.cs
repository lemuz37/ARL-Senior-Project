using Assimp;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace UnBox3D.Models
{
    public class ModelLoader
    {
        public List<Vector3> Vertices { get; private set; } = new();

        public bool LoadModel(string path)
        {
            try
            {
                using var importer = new AssimpContext();
                var scene = importer.ImportFile(path, PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs);

                if (scene == null || scene.SceneFlags == SceneFlags.Incomplete || scene.RootNode == null)
                    return false;

                foreach (var mesh in scene.Meshes)
                {
                    foreach (var vertex in mesh.Vertices)
                    {
                        Vertices.Add(new Vector3(vertex.X, vertex.Y, vertex.Z));
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading model: {ex.Message}");
                return false;
            }
        }
    }
}
