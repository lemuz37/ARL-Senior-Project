using OpenTK.Mathematics;
using Assimp;
using g3;

namespace UnBox3D.Rendering
{
    public class GeometryGenerator
    {
        public static AppMesh CreateBox(Vector3 center, float width, float height, float depth)
        {
            Mesh assimpMesh = new Mesh("Box", PrimitiveType.Triangle);
            DMesh3 g3Mesh = new DMesh3();

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
                // Add vertices to g3Mesh
                g3Mesh.AppendVertex(new g3.Vector3d(v.X, v.Y, v.Z));
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

            foreach (var face in faces)
            {
                assimpMesh.Faces.Add(new Face(face));

                // Add faces to g3Mesh
                g3Mesh.AppendTriangle(face[0], face[1], face[2]);
            }
            return new AppMesh(g3Mesh, assimpMesh);
        }

        public static AppMesh CreateCylinder(Vector3 center, float radius, float height, int segments)
        {
            Mesh assimpMesh = new Mesh("Cylinder", PrimitiveType.Triangle);
            DMesh3 g3Mesh = new DMesh3();
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

            // Add vertices to g3 mesh
            foreach (var v in vertices)
            {
                g3Mesh.AppendVertex(new g3.Vector3d(v.X, v.Y, v.Z));
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

                g3Mesh.AppendTriangle(bottomCurrent, topCurrent, bottomNext);
                g3Mesh.AppendTriangle(bottomNext, topCurrent, topNext);

                // Bottom cap
                assimpMesh.Faces.Add(new Face([segments * 2, bottomNext, bottomCurrent]));
                g3Mesh.AppendTriangle(segments * 2, bottomNext, bottomCurrent);

                // Top cap
                assimpMesh.Faces.Add(new Face([segments * 2 + 1, topCurrent, topNext]));
                g3Mesh.AppendTriangle(segments * 2 + 1, topCurrent, topNext);
            }
            return new AppMesh(g3Mesh, assimpMesh);
        }
    }
}
