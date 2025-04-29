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

            Vector3[] vertices =
            [
                new Vector3(center.X - width / 2, center.Y - height / 2, center.Z - depth / 2),
                new Vector3(center.X + width / 2, center.Y - height / 2, center.Z - depth / 2),
                new Vector3(center.X + width / 2, center.Y + height / 2, center.Z - depth / 2),
                new Vector3(center.X - width / 2, center.Y + height / 2, center.Z - depth / 2),

                new Vector3(center.X - width / 2, center.Y - height / 2, center.Z + depth / 2),
                new Vector3(center.X + width / 2, center.Y - height / 2, center.Z + depth / 2),
                new Vector3(center.X + width / 2, center.Y + height / 2, center.Z + depth / 2),
                new Vector3(center.X - width / 2, center.Y + height / 2, center.Z + depth / 2)
            ];

            foreach (var v in vertices)
            {
                assimpMesh.Vertices.Add(new Assimp.Vector3D(v.X, v.Y, v.Z));
                g4Mesh.AppendVertex(new g4.Vector3d(v.X, v.Y, v.Z));
            }

            int[][] faces =
            [
                [0, 2, 1], [0, 3, 2],
                [5, 7, 4], [5, 6, 7],
                [4, 3, 0], [4, 7, 3],
                [1, 2, 6], [1, 6, 5],
                [3, 7, 6], [3, 6, 2],
                [4, 0, 1], [4, 1, 5]
            ];


            Vector3[] vertexNormals = new Vector3[vertices.Length];

            foreach (var face in faces)
            {
                Vector3 v0 = vertices[face[0]];
                Vector3 v1 = vertices[face[1]];
                Vector3 v2 = vertices[face[2]];

                Vector3 edge1 = v1 - v0;
                Vector3 edge2 = v2 - v0;

                Vector3 faceNormal = Vector3.Cross(edge1, edge2).Normalized();

                vertexNormals[face[0]] += faceNormal;
                vertexNormals[face[1]] += faceNormal;
                vertexNormals[face[2]] += faceNormal;
                assimpMesh.Faces.Add(new Face(face));

                g4Mesh.AppendTriangle(face[0], face[1], face[2]);
            }

            MeshNormals.QuickCompute(g4Mesh);

            assimpMesh.Normals.Clear();
            foreach (var vid in g4Mesh.VertexIndices())
            {
                var n = g4Mesh.GetVertexNormal(vid);
                assimpMesh.Normals.Add(new Assimp.Vector3D((float)n.x, (float)n.y, (float)n.z));
            }

            AppMesh mesh = new AppMesh(g4Mesh, assimpMesh);
            mesh.SetColor(Colors.Red);

            return mesh;
        }

        public static AppMesh CreateCylinder(Vector3 center, float radius, float height, int segments)
        {
            Mesh assimpMesh = new Mesh("Cylinder", PrimitiveType.Triangle);
            DMesh3 g4Mesh = new DMesh3();
            float halfHeight = height * 0.5f;
            List<Vector3> vertices = new List<Vector3>();

            for (int i = 0; i < segments; i++)
            {
                float angle = i * ((float)Math.PI * 2 / segments);
                float x = (float)Math.Cos(angle) * radius;
                float z = (float)Math.Sin(angle) * radius;

                vertices.Add(new Vector3(center.X + x, center.Y - halfHeight, center.Z + z));
                vertices.Add(new Vector3(center.X + x, center.Y + halfHeight, center.Z + z));
            }

            Vector3 bottomCenter = new Vector3(center.X, center.Y - halfHeight, center.Z);
            Vector3 topCenter = new Vector3(center.X, center.Y + halfHeight, center.Z);
            vertices.Add(bottomCenter);
            vertices.Add(topCenter);

            foreach (var v in vertices)
            {
                assimpMesh.Vertices.Add(new Assimp.Vector3D(v.X, v.Y, v.Z));
            }

            foreach (var v in vertices)
            {
                g4Mesh.AppendVertex(new g4.Vector3d(v.X, v.Y, v.Z));
            }

            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;

                int bottomCurrent = i * 2;
                int topCurrent = bottomCurrent + 1;
                int bottomNext = next * 2;
                int topNext = bottomNext + 1;

                assimpMesh.Faces.Add(new Face([bottomCurrent, topCurrent, bottomNext]));
                assimpMesh.Faces.Add(new Face([bottomNext, topCurrent, topNext]));

                g4Mesh.AppendTriangle(bottomCurrent, topCurrent, bottomNext);
                g4Mesh.AppendTriangle(bottomNext, topCurrent, topNext);

                assimpMesh.Faces.Add(new Face([segments * 2, bottomNext, bottomCurrent]));
                g4Mesh.AppendTriangle(segments * 2, bottomNext, bottomCurrent);

                assimpMesh.Faces.Add(new Face([segments * 2 + 1, topCurrent, topNext]));
                g4Mesh.AppendTriangle(segments * 2 + 1, topCurrent, topNext);
            }

            MeshNormals.QuickCompute(g4Mesh);

            assimpMesh.Normals.Clear();
            foreach (var vid in g4Mesh.VertexIndices())
            {
                var n = g4Mesh.GetVertexNormal(vid);
                assimpMesh.Normals.Add(new Assimp.Vector3D((float)n.x, (float)n.y, (float)n.z));
            }

            var mesh = new AppMesh(g4Mesh, assimpMesh);
            mesh.SetColor(Colors.Red);

            return mesh;
        }
    }
}
