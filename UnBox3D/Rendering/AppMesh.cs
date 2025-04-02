using Assimp;
using g3;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using UnBox3D.Rendering.OpenGL;
using Quaternion = OpenTK.Mathematics.Quaternion;

namespace UnBox3D.Rendering
{
    public interface IAppMesh
    {
        // Properties
        //Vector3 GetCenter();
        //Vector3 GetDimensions();
        DMesh3 GetG3Mesh();
        Mesh GetAssimpMesh();
        string GetName();
        int VertexCount { get; }
        Vector3 GetColor();
        bool GetHighlighted();
        float[] Vertices { get; }
        List<Vector3> GetEdges();
        Quaternion GetTransform();

        // Actions
        void SetColor(Vector3 color);
        //void Move(Vector3 translation);
        void Rotate(Quaternion rotation);

        // OpenGL Handles
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
        private float[] _vertices;
        private List<Vector3> _edges;
        private int _vao, _vertexBufferObject;
        private bool _disposed = false;
        private Quaternion _transform = Quaternion.Identity;

        // Constructor
        public AppMesh(DMesh3 g3mesh, Mesh assimpMesh)
        {
            _g3Mesh = g3mesh;
            _assimpMesh = assimpMesh;
            _name = assimpMesh.Name;
            _edges = new List<Vector3>();

            // Populate vertices float array
            if (assimpMesh.HasNormals)
            {
                _vertices = new float[assimpMesh.VertexCount * 6];
                for (int i = 0; i < assimpMesh.VertexCount; i++)
                {
                    int index = i * 6;
                    _vertices[index] = assimpMesh.Vertices[i].X;
                    _vertices[index + 1] = assimpMesh.Vertices[i].Z;
                    _vertices[index + 2] = assimpMesh.Vertices[i].Y;
                    _vertices[index + 3] = assimpMesh.Normals[i].X;
                    _vertices[index + 4] = assimpMesh.Normals[i].Z;
                    _vertices[index + 5] = assimpMesh.Normals[i].Y;
                }
            }
            else
            {
                _vertices = new float[assimpMesh.VertexCount * 3];
                for (int i = 0; i < assimpMesh.VertexCount; i++)
                {
                    int index = i * 3;
                    _vertices[index] = assimpMesh.Vertices[i].X;
                    _vertices[index + 1] = assimpMesh.Vertices[i].Z;
                    _vertices[index + 2] = assimpMesh.Vertices[i].Y;
                }
            }

            // Populate edges
            for (int i = 0; i < _g3Mesh.EdgeCount; i++)
            {
                Index2i edgeVertices = _g3Mesh.GetEdgeV(i);

                _edges.Add(new Vector3(
                    (float)_g3Mesh.GetVertex(edgeVertices.a).x,
                    (float)_g3Mesh.GetVertex(edgeVertices.a).y,
                    (float)_g3Mesh.GetVertex(edgeVertices.a).z));

                _edges.Add(new Vector3(
                    (float)_g3Mesh.GetVertex(edgeVertices.b).x,
                    (float)_g3Mesh.GetVertex(edgeVertices.b).y,
                    (float)_g3Mesh.GetVertex(edgeVertices.b).z));
            }

            SetupMesh();
        }

        // OpenGL Setup
        private void SetupMesh()
        {
            Shader _lightingShader = new Shader("Rendering/OpenGL/Shaders/shader.vert", "Rendering/OpenGL/Shaders/lighting.frag");

            _vertexBufferObject = GL.GenBuffer(); // Generates a unique ID for a new OpenGL buffer object
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);
            var positionLocation = _lightingShader.GetAttribLocation("aPos");
            GL.EnableVertexAttribArray(positionLocation);
            if (_assimpMesh.HasNormals)
            {
                // Remember to change the stride as we now have 6 floats per vertex
                GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
                // We now need to define the layout of the normal so the shader can use it
                var normalLocation = _lightingShader.GetAttribLocation("aNormal");
                GL.EnableVertexAttribArray(normalLocation);
                GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            }
            else 
            {
                // Remember to change the stride as we now have 3 floats per vertex
                GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            }
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        // Properties
        public string Name => GetName();
        public int VertexCount => _assimpMesh.VertexCount;
        public float[] Vertices => _vertices;
        public int GetVAO() => _vao;
        public int GetVBO() => _vertexBufferObject;

        // Getters
        public DMesh3 GetG3Mesh() => _g3Mesh;
        public Mesh GetAssimpMesh() => _assimpMesh;
        public string GetName() => _name;
        public Vector3 GetColor() => _color;
        public bool GetHighlighted() => _highlighted;
        public float[] GetVertices() => _vertices;
        public List<Vector3> GetEdges() => _edges;
        public Quaternion GetTransform() => _transform;

        // Setters
        public void SetName(string name) => _name = name;
        public void SetColor(Vector3 color) => _color = color;
        public void SetHighlighted(bool flag) => _highlighted = flag;

        // Transformations
        //public void Move(Vector3 translation)
        //{
        //    for (int i = 0; i < _vertices.Length; i++)
        //    {
        //        _vertices[i] += translation;
        //    }
        //}

        public void Rotate(Quaternion rotation)
        {
            _transform = rotation * _transform;
            _transform.Normalize();
        }



        // Bounding Box Calculations
        //public Vector3 GetCenter()
        //{
        //    if (_vertices.Count == 0) return Vector3.Zero;
        //    var min = _vertices.Aggregate(Vector3.ComponentMin);
        //    var max = _vertices.Aggregate(Vector3.ComponentMax);
        //    return (min + max) * 0.5f;
        //}

        //public Vector3 GetDimensions()
        //{
        //    if (_vertices.Count == 0) return Vector3.Zero;
        //    var min = _vertices.Aggregate(Vector3.ComponentMin);
        //    var max = _vertices.Aggregate(Vector3.ComponentMax);
        //    return max - min;
        //}

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
                //_vertices?.Clear();
                _edges?.Clear();
                //_normals?.Clear();
            }
            _disposed = true;
        }
    }
}
