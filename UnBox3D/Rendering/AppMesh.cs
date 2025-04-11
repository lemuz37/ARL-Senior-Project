﻿using Assimp;
using g3;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using UnBox3D.Rendering.OpenGL;
using Quaternion = OpenTK.Mathematics.Quaternion;

namespace UnBox3D.Rendering
{
    public interface IAppMesh
    {
        #region Properties
        DMesh3 GetG3Mesh();
        Mesh GetAssimpMesh();
        string Name { get; set; }
        int VertexCount { get; }
        Vector3 GetColor();
        bool GetHighlighted();
        float[] Vertices { get; }
        List<Vector3> GetEdges();
        Quaternion GetTransform();
        int[] GetIndices();

        #endregion

        #region Actions
        void SetColor(Vector3 color);
        void Rotate(Quaternion rotation);
        #endregion

        #region OpenGL Handles
        int GetVAO();
        int GetVBO();
        #endregion
    }

    public class AppMesh : IAppMesh, IDisposable
    {
        #region Fields
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
        private int _ebo;
        private int[] _indices;
        #endregion

        #region Constructor
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

            // Populate indices
            _indices = assimpMesh.GetIndices();

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
        #endregion

        #region OpenGL Setup
        private void SetupMesh()
        {
            Shader _lightingShader = ShaderManager.LightingShader;

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            _ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(int), _indices, BufferUsageHint.StaticDraw);

            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);

            var positionLocation = _lightingShader.GetAttribLocation("aPos");
            GL.EnableVertexAttribArray(positionLocation);

            if (_assimpMesh.HasNormals)
            {
                GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
                var normalLocation = _lightingShader.GetAttribLocation("aNormal");
                GL.EnableVertexAttribArray(normalLocation);
                GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            }
            else
            {
                GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }
        #endregion

        #region Properties
        public string Name
        {
            get => _name;
            set => _name = value;
        }
        public int VertexCount => _assimpMesh.VertexCount;
        public float[] Vertices => _vertices;
        public int GetVAO() => _vao;
        public int GetVBO() => _vertexBufferObject;
        #endregion

        #region Getters
        public DMesh3 GetG3Mesh() => _g3Mesh;
        public Mesh GetAssimpMesh() => _assimpMesh;
        public Vector3 GetColor() => _color;
        public bool GetHighlighted() => _highlighted;
        public List<Vector3> GetEdges() => _edges;
        public Quaternion GetTransform() => _transform;
        public int[] GetIndices() => _indices;

        #endregion

        #region Setters
        public void SetName(string name) => _name = name;
        public void SetColor(Vector3 color) => _color = color;
        public void SetHighlighted(bool flag) => _highlighted = flag;
        #endregion

        #region Transformations
        public void Rotate(Quaternion rotation)
        {
            _transform = rotation * _transform;
            _transform.Normalize();
        }
        #endregion

        #region Cleanup & Disposal
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
                _edges?.Clear();
            }
            _disposed = true;
        }
        #endregion
    }
}
