using OpenTK.Mathematics;
using Assimp;
using g4;
using UnBox3D.Utils;

namespace UnBox3D.Rendering
{
    public class GeometryGenerator
    {
        public static AppMesh CreateBox(Vector3 center, float width, float height, float depth, string name = "Box")
        {
            Mesh assimpMesh = new Mesh(name, PrimitiveType.Triangle);
            DMesh3 g4Mesh = new DMesh3();

            // Define box vertices
            Vector3[] vertices =
            [
                new Vector3(center.X - width / 2, center.Y - height / 2, center.Z - depth / 2), // 0
                new Vector3(center.X + width / 2, center.Y - height / 2, center.Z - depth / 2), // 1
                new Vector3(center.X + width / 2, center.Y + height / 2, center.Z - depth / 2), // 2
                new Vector3(center.X - width / 2, center.Y + height / 2, center.Z - depth / 2), // 3

                new Vector3(center.X - width / 2, center.Y - height / 2, center.Z + depth / 2), // 4
                new Vector3(center.X + width / 2, center.Y - height / 2, center.Z + depth / 2), // 5
                new Vector3(center.X + width / 2, center.Y + height / 2, center.Z + depth / 2), // 6
                new Vector3(center.X - width / 2, center.Y + height / 2, center.Z + depth / 2)  // 7
            ];

            // Add vertices to Assimp Mesh
            foreach (var v in vertices)
            {
                assimpMesh.Vertices.Add(new Assimp.Vector3D(v.X, v.Y, v.Z));

                // Create Equivalent DMesh3
                // Add vertices to g4Mesh
                g4Mesh.AppendVertex(new g4.Vector3d(v.X, v.Y, v.Z));
            }

            // Define box faces (two triangles per face)
            int[][] faces =
            [
                [0, 1, 2], [0, 2, 3], // Front
                [5, 4, 7], [5, 7, 6], // Back
                [4, 0, 3], [4, 3, 7], // Left
                [1, 5, 6], [1, 6, 2], // Right
                [3, 2, 6], [3, 6, 7], // Top
                [4, 5, 1], [4, 1, 0]  // Bottom
            ];

            // Initialize empty normals for each vertex
            Vector3[] vertexNormals = new Vector3[vertices.Length];

            foreach (var face in faces)
            {
                Vector3 v0 = vertices[face[0]];
                Vector3 v1 = vertices[face[1]];
                Vector3 v2 = vertices[face[2]];

                Vector3 edge1 = v1 - v0;
                Vector3 edge2 = v2 - v0;

                Vector3 faceNormal = Vector3.Cross(edge1, edge2).Normalized();

                // Accumulate normals at each vertex of the face
                vertexNormals[face[0]] += faceNormal;
                vertexNormals[face[1]] += faceNormal;
                vertexNormals[face[2]] += faceNormal;
                assimpMesh.Faces.Add(new Face(face));

                // Add faces to g4Mesh
                g4Mesh.AppendTriangle(face[0], face[1], face[2]);
            }

            // Normalize accumulated vertex normals and add to Assimp
            foreach (var normal in vertexNormals)
            {
                var n = normal.Normalized();
                assimpMesh.Normals.Add(new Assimp.Vector3D(n.X, n.Y, n.Z));
            }

            AppMesh appMesh = new AppMesh(g4Mesh, assimpMesh);
            appMesh.SetColor(Colors.Red);

            return appMesh;
        }

        public static AppMesh CreateCylinder(Vector3 center, float radius, float height, int segments)
        {
            Mesh assimpMesh = new Mesh("Cylinder", PrimitiveType.Triangle);
            DMesh3 g4Mesh = new DMesh3();
            float halfHeight = height * 0.5f;
            List<Vector3> vertices = new List<Vector3>();

            // Generate vertices for the cylinder
            for (int i = 0; i < segments; i++)
            {
                float angle = i * ((float)Math.PI * 2 / segments);
                float x = (float)Math.Cos(angle) * radius;
                float z = (float)Math.Sin(angle) * radius;

                // Bottom ring
                vertices.Add(new Vector3(center.X + x, center.Y - halfHeight, center.Z + z));
                // Top ring
                vertices.Add(new Vector3(center.X + x, center.Y + halfHeight, center.Z + z));
            }

            // Bottom and top center points
            Vector3 bottomCenter = new Vector3(center.X, center.Y - halfHeight, center.Z);
            Vector3 topCenter = new Vector3(center.X, center.Y + halfHeight, center.Z);
            vertices.Add(bottomCenter);
            vertices.Add(topCenter);

            // Add vertices to Assimp mesh
            foreach (var v in vertices)
            {
                assimpMesh.Vertices.Add(new Assimp.Vector3D(v.X, v.Y, v.Z));
            }

            // Add vertices to g4 mesh
            foreach (var v in vertices)
            {
                g4Mesh.AppendVertex(new g4.Vector3d(v.X, v.Y, v.Z));
            }

            // Define cylinder faces (triangles)
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;

                // Side faces (two triangles per segment)
                int bottomCurrent = i * 2;
                int topCurrent = bottomCurrent + 1;
                int bottomNext = next * 2;
                int topNext = bottomNext + 1;

                // Side triangles
                assimpMesh.Faces.Add(new Face([bottomCurrent, topCurrent, bottomNext]));
                assimpMesh.Faces.Add(new Face([bottomNext, topCurrent, topNext]));

                g4Mesh.AppendTriangle(bottomCurrent, topCurrent, bottomNext);
                g4Mesh.AppendTriangle(bottomNext, topCurrent, topNext);

                // Bottom cap
                assimpMesh.Faces.Add(new Face([segments * 2, bottomNext, bottomCurrent]));
                g4Mesh.AppendTriangle(segments * 2, bottomNext, bottomCurrent);

                // Top cap
                assimpMesh.Faces.Add(new Face([segments * 2 + 1, topCurrent, topNext]));
                g4Mesh.AppendTriangle(segments * 2 + 1, topCurrent, topNext);
            }
            return new AppMesh(g4Mesh, assimpMesh);
        }
    }
}
