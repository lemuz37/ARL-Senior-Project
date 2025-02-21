using Assimp;
using g3;
using OpenTK.Mathematics;
using Quaternion = OpenTK.Mathematics.Quaternion;

namespace UnBox3D.Rendering
{
    public interface IAppMesh 
    {
        Vector3 GetCenter();
        Vector3 GetDimensions();
        public DMesh3 GetG3Mesh();
        public Mesh GetAssimpMesh();
        public string GetName();
        public Vector3 GetColor();
        public bool GetHighlighted();
        public List<Vector3> GetNormals();
        public List<Vector3> GetVertices();
        public List<Vector3> GetEdges();
        public Quaternion GetTransform();
        void Move(Vector3 translation);
        void Rotate(Quaternion rotationMatrix);
    }
    public class AppMesh : IAppMesh, IDisposable
    {
        private DMesh3 _g3Mesh;
        private Mesh _assimpMesh;
        private string _name;
        private bool _highlighted = false;
        private Vector3 _color;
        private List<Vector3> _normals;
        private List<Vector3> _vertices;
        private List<Vector3> _edges;
        private Gizmo _gizmo;
        private bool _disposed = false;
        private Quaternion _transform = Quaternion.Identity;

        public string Name => GetName();

        public List<string> Vertices
        {
            get
            {
                return _g3Mesh.Vertices()
                    .Select(v => $"({v.x:F2}, {v.y:F2}, {v.z:F2})")
                    .ToList();
            }
        }

        public AppMesh(DMesh3 g3mesh, Mesh assimpMesh)
        {
            SetMesh(g3mesh, assimpMesh);
        }

        // Getters
        public DMesh3 GetG3Mesh() 
        {
            return _g3Mesh;
        }

        public Mesh GetAssimpMesh() 
        {
            return _assimpMesh;
        }

        public string GetName() 
        {
            return _name;
        }

        public Vector3 GetColor() 
        {
            return _color;
        }
        public bool GetHighlighted() 
        {
            return _highlighted;
        }
        public List<Vector3> GetNormals() 
        {
            return _normals;
        }
        public List<Vector3> GetVertices()
        {
            return _vertices;
        }
        public List<Vector3> GetEdges()
        {
            return _edges;
        }

        public Quaternion GetTransform()
        {
            return _transform;
        }

        public Vector3 GetCenter()
        {
            if (_assimpMesh.VertexCount == 0) return Vector3.Zero;

            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            foreach (var vertex in _vertices)
            {
                Vector3 vertexVec = new Vector3(vertex.X, vertex.Y, vertex.X);
                min = Vector3.ComponentMin(min, vertexVec);
                max = Vector3.ComponentMax(max, vertexVec);
            }

            return (min + max) * 0.5f;
        }

        public Vector3 GetDimensions()
        {
            if (_assimpMesh.VertexCount == 0) return Vector3.Zero;

            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            foreach (var vertex in _vertices)
            {
                Vector3 vertexVec = new Vector3(vertex.X, vertex.Y, vertex.Z);
                min = Vector3.ComponentMin(min, vertexVec);
                max = Vector3.ComponentMax(max, vertexVec);
            }

            return max - min;
        }

        // Setters
        public void SetMesh(DMesh3 g3mesh, Mesh assimpMesh) 
        {
            _g3Mesh = g3mesh;
            _assimpMesh = assimpMesh;
            SetName(assimpMesh.Name);
            _normals = new List<Vector3>();
            _vertices = new List<Vector3>();
            _edges = new List<Vector3>();

            // Populate vertices and edges
            for (int i = 0; i < assimpMesh.VertexCount; i++)
            {
                Vector3 vertex = new Vector3(_assimpMesh.Vertices[i].X, _assimpMesh.Vertices[i].Y, _assimpMesh.Vertices[i].Z);
                _vertices.Add(vertex);
            }

            for (int i = 0; i < _g3Mesh.EdgeCount; i++)
            {
                Index2i edgeTriangles = _g3Mesh.GetEdgeT(i);
                if (edgeTriangles.b == DMesh3.InvalidID)  // Boundary edge
                {
                    Index2i edgeVertices = _g3Mesh.GetEdgeV(i);
                    g3.Vector3d v0 = _g3Mesh.GetVertex(edgeVertices.a);
                    g3.Vector3d v1 = _g3Mesh.GetVertex(edgeVertices.b);

                    Vector3 vertex0 = new Vector3((float)v0.x, (float)v0.y, (float)v0.z);
                    Vector3 vertex1 = new Vector3((float)v1.x, (float)v1.y, (float)v1.z);

                    _edges.Add(vertex0);
                    _edges.Add(vertex1);
                }
            }

            CalculateNormals();
        }

        public void SetName(string name) 
        {
            _name = name;
        }
        public void SetColor(Vector3 color) 
        {
            _color = color;
        }
        public void SetHighlighted(bool flag) 
        {
            _highlighted = flag;
        }


        // Actions
        public void Move(Vector3 translation)
        {
            for (int i = 0; i < _vertices.Count; i++)
            {
                _vertices[i] += translation;

                var g3Vertex = _g3Mesh.GetVertex(i);
                _g3Mesh.SetVertex(i, new g3.Vector3d(g3Vertex.x + translation.X, g3Vertex.y + translation.Y, g3Vertex.z + translation.Z));

                var assimpVertex = _assimpMesh.Vertices[i];
                _assimpMesh.Vertices[i] = new Assimp.Vector3D(
                    assimpVertex.X + translation.X,
                    assimpVertex.Y + translation.Y,
                    assimpVertex.Z + translation.Z
                );
            }
        }

        public void Rotate(Quaternion rotation)
        {
            // Update the overall transformation quaternion
            _transform = rotation * _transform;

            // Normalize the transformation quaternion once
            Quaternion normalizedRotation = _transform.Normalized();

            for (int i = 0; i < _vertices.Count; i++)
            {
                // Convert the vertex to a quaternion (w = 0)
                Quaternion vectorQuat = new Quaternion(_vertices[i].X, _vertices[i].Y, _vertices[i].Z, 0);

                // Apply the rotation: result = normalizedRotation * vectorQuat * normalizedRotation^-1
                Quaternion rotatedQuat = normalizedRotation * vectorQuat * normalizedRotation.Inverted();

                // Extract the rotated vector
                Vector3 rotatedVertex = new Vector3(rotatedQuat.X, rotatedQuat.Y, rotatedQuat.Z);

                // Update all representations
                _vertices[i] = rotatedVertex;
                _g3Mesh.SetVertex(i, new g3.Vector3d(rotatedVertex.X, rotatedVertex.Y, rotatedVertex.Z));
                _assimpMesh.Vertices[i] = new Assimp.Vector3D(rotatedVertex.X, rotatedVertex.Y, rotatedVertex.Z);
            }
        }


        private void CalculateNormals()
        {
            // Ensure _normals list matches the number of vertices
            _normals.Clear();
            for (int i = 0; i < _vertices.Count; i++)
            {
                _normals.Add(Vector3.Zero);
                _assimpMesh.Normals.Add(new Assimp.Vector3D(0, 0, 0)); // Clear and initialize Assimp normals
            }

            // Compute normals per face
            foreach (var face in _assimpMesh.Faces)
            {
                // Get the vertices of the face
                var v0 = _assimpMesh.Vertices[face.Indices[0]];
                var v1 = _assimpMesh.Vertices[face.Indices[1]];
                var v2 = _assimpMesh.Vertices[face.Indices[2]];

                // Calculate the face normal
                var edge1 = v1 - v0;
                var edge2 = v2 - v0;
                var normal = Assimp.Vector3D.Cross(edge1, edge2);
                normal.Normalize();

                // Add the face normal to each vertex in the face
                foreach (var index in face.Indices)
                {
                    // Update Assimp normals
                    _assimpMesh.Normals[index] += normal;

                    // Update local _normals list
                    _normals[index] += new Vector3(normal.X, normal.Y, normal.Z);
                }
            }

            // Normalize all vertex normals
            for (int i = 0; i < _normals.Count; i++)
            {
                // Normalize the local normals
                _normals[i] = _normals[i].Normalized();

                // Normalize the Assimp normals
                _assimpMesh.Normals[i].Normalize();
            }
        }

        /// <summary>
        /// Frees resources when the object is disposed.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); // Prevent finalizer from running
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // Free managed resources
                _vertices?.Clear();
                _edges?.Clear();
                _normals?.Clear();
            }

            // Free unmanaged resources here if necessary

            _disposed = true;
        }

        /// <summary>
        /// Finalizer to ensure Dispose is called
        /// </summary>
        ~AppMesh()
        {
            Dispose(false);
        }
    }
}
