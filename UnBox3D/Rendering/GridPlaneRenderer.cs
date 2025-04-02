using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using UnBox3D.Rendering.OpenGL;

namespace UnBox3D.Rendering
{
    public class GridPlaneRenderer
    {
        private int _vao, _vbo;
        private Shader _shader;
        private int _gridLines;
        private float _gridSize;
        private float _transparency;

        public GridPlaneRenderer(string vertexShaderPath, string fragmentShaderPath)
        {
            _gridSize = 1000.0f;
            _gridLines = 100;
            _transparency = 0.3f;

            // Load Shader using your Shader class
            _shader = new Shader(vertexShaderPath, fragmentShaderPath);

            InitializeGrid();
        }

        private void InitializeGrid()
        {
            float[] vertices = GenerateGridVertices(_gridSize, _gridLines);

            // Generate VAO & VBO
            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            // Define vertex attributes (position only)
            int positionLocation = _shader.GetAttribLocation("aPos");
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(positionLocation);

            GL.BindVertexArray(0);
        }

        private float[] GenerateGridVertices(float gridSize, int gridLines)
        {
            int numLines = (gridLines * 4) + 4;
            float[] vertices = new float[numLines * 6];

            int index = 0;
            float step = gridSize / gridLines;

            // Vertical lines (X direction)
            for (int i = -gridLines; i <= gridLines; i++)
            {
                float x = i * step;
                vertices[index++] = x; vertices[index++] = 0; vertices[index++] = -gridSize;
                vertices[index++] = x; vertices[index++] = 0; vertices[index++] = gridSize;
            }

            // Horizontal lines (Z direction)
            for (int i = -gridLines; i <= gridLines; i++)
            {
                float z = i * step;
                vertices[index++] = -gridSize; vertices[index++] = 0; vertices[index++] = z;
                vertices[index++] = gridSize; vertices[index++] = 0; vertices[index++] = z;
            }

            return vertices;
        }

        public void DrawGrid(Matrix4 view, Matrix4 projection)
        {
            //GL.Enable(EnableCap.Blend);
            //GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            //GL.Disable(EnableCap.DepthTest);

            _shader.Use();

            _shader.SetMatrix4("view", view);
            _shader.SetMatrix4("projection", projection);
            _shader.SetVector3("objectColor", new Vector3(0.5f, 0.5f, 0.5f)); // Light gray color

            // Bind VAO and draw
            GL.BindVertexArray(_vao);
            GL.DrawArrays(PrimitiveType.Lines, 0, (_gridLines * 4) + 4);
            GL.BindVertexArray(0);

            GL.UseProgram(0);

            GL.Enable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Blend);
        }

        public void Cleanup()
        {
            GL.DeleteVertexArray(_vao);
            GL.DeleteBuffer(_vbo);
            GL.DeleteProgram(_shader.Handle);
        }
    }
}
