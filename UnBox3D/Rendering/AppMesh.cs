using Assimp;
using g3;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Quaternion = OpenTK.Mathematics.Quaternion;

namespace UnBox3D.Rendering
{
    public interface IAppMesh
    {
        // Properties
        Vector3 GetCenter();
        Vector3 GetDimensions();
        DMesh3 GetG3Mesh();
        Mesh GetAssimpMesh();
        string GetName();
        int VertexCount { get; }
        Vector3 GetColor();
        bool GetHighlighted();
        List<Vector3> GetNormals();
        List<Vector3> Vertices { get; }
        List<Vector3> GetEdges();
        Quaternion GetTransform();

        // Actions
        void SetColor(Vector3 color);
        void Move(Vector3 translation);
        void Rotate(Quaternion rotation);

        // OpenGL Handles
        void SetupMesh();
        int GetVAO();
        int GetVBO();
    }

    public class AppMesh : IAppMesh, IDisposable
    {
        // Fields
        private DMesh3 _g3Mesh;
        private Mesh _assimpMesh;
        private string _name;
        private bool _highlighted = false;
        private Vector3 _color;
        private List<Vector3> _normals;
        private List<Vector3> _vertices;
        private List<Vector3> _edges;
        private int _vao, _vbo;
        private bool _disposed = false;
        private Quaternion _transform = Quaternion.Identity;

        // Constructor
        public AppMesh(DMesh3 g3mesh, Mesh assimpMesh)
        {
            SetMesh(g3mesh, assimpMesh);
        }

        // Properties
        public string Name => GetName();
        public int VertexCount => _vertices.Count;
        public List<Vector3> Vertices => _vertices;
        public int GetVAO() => _vao;
        public int GetVBO() => _vbo;

        // Getters
        public DMesh3 GetG3Mesh() => _g3Mesh;
        public Mesh GetAssimpMesh() => _assimpMesh;
        public string GetName() => _name;
        public Vector3 GetColor() => _color;
        public bool GetHighlighted() => _highlighted;
        public List<Vector3> GetNormals() => _normals;
        public List<Vector3> GetVertices() => _vertices;
        public List<Vector3> GetEdges() => _edges;
        public Quaternion GetTransform() => _transform;

        // Setters
        public void SetName(string name) => _name = name;
        public void SetColor(Vector3 color) => _color = color;
        public void SetHighlighted(bool flag) => _highlighted = flag;

        // Mesh Setup
        public void SetMesh(DMesh3 g3mesh, Mesh assimpMesh)
        {
            _g3Mesh = g3mesh;
            _assimpMesh = assimpMesh;
            SetName(assimpMesh.Name);
            _vertices = new List<Vector3>();
            _edges = new List<Vector3>();
            _normals = new List<Vector3>();

            // Populate vertices
            foreach (var vertex in assimpMesh.Vertices)
            {
                _vertices.Add(new Vector3(vertex.X, vertex.Y, vertex.Z));
            }

            // Populate edges
            for (int i = 0; i < _g3Mesh.EdgeCount; i++)
            {
                Index2i edgeVertices = _g3Mesh.GetEdgeV(i);
                _edges.Add(new Vector3((float)_g3Mesh.GetVertex(edgeVertices.a).x,
                                        (float)_g3Mesh.GetVertex(edgeVertices.a).y,
                                        (float)_g3Mesh.GetVertex(edgeVertices.a).z));
                _edges.Add(new Vector3((float)_g3Mesh.GetVertex(edgeVertices.b).x,
                                        (float)_g3Mesh.GetVertex(edgeVertices.b).y,
                                        (float)_g3Mesh.GetVertex(edgeVertices.b).z));
            }

            CalculateNormals();
        }

        // Transformations
        public void Move(Vector3 translation)
        {
            for (int i = 0; i < _vertices.Count; i++)
            {
                _vertices[i] += translation;
            }
        }

        public void Rotate(Quaternion rotation)
        {
            _transform = rotation * _transform;
            _transform.Normalize();
        }

        // Normal Calculation
        private void CalculateNormals()
        {
            _normals.Clear();
            for (int i = 0; i < _vertices.Count; i++)
            {
                _normals.Add(Vector3.Zero);
            }
        }

        // OpenGL Setup
        public void SetupMesh()
        {
            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Count * Vector3.SizeInBytes, _vertices.ToArray(), BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vector3.SizeInBytes, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        // Bounding Box Calculations
        public Vector3 GetCenter()
        {
            if (_vertices.Count == 0) return Vector3.Zero;
            var min = _vertices.Aggregate(Vector3.ComponentMin);
            var max = _vertices.Aggregate(Vector3.ComponentMax);
            return (min + max) * 0.5f;
        }

        public Vector3 GetDimensions()
        {
            if (_vertices.Count == 0) return Vector3.Zero;
            var min = _vertices.Aggregate(Vector3.ComponentMin);
            var max = _vertices.Aggregate(Vector3.ComponentMax);
            return max - min;
        }

        // Cleanup & Disposal
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                _vertices?.Clear();
                _edges?.Clear();
                _normals?.Clear();
            }
            _disposed = true;
        }
    }
}
