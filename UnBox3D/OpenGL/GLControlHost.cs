using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL4;
using System.IO;

namespace UnBox3D.OpenGL
{
    public class GLControlHost : GLControl
    {
        private readonly float[] _vertices =
        {
            -0.5f, -0.5f, 0.0f, // Bottom-left vertex
             0.5f, -0.5f, 0.0f, // Bottom-right vertex
             0.0f,  0.5f, 0.0f  // Top vertex
        };

        private int _vertexBufferObject;
        private int _vertexArrayObject;
        private Shader _shader;
        private bool _initialized = false;

        public GLControlHost() : base()
        {
            Dock = DockStyle.Fill;
            Load += OnLoad;
            Paint += OnRender;
            Resize += OnResize;
            Application.Idle += OnUpdate;
        }

        private void OnLoad(object sender, EventArgs e)
        {
            if (_initialized) return;
            // OpenGL Initialization
            MakeCurrent();
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            // Generate and bind a Vertex Buffer Object (VBO)
            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            // Generate and bind a Vertex Array Object (VAO)
            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // Load and compile shaders
            _shader = new Shader("OpenGL/Shaders/shader.vert", "OpenGL/Shaders/shader.frag");
            _shader.Use();

            _initialized = true;
        }

        private void OnRender(object sender, PaintEventArgs e)
        {
            if (!Context.IsCurrent) MakeCurrent();

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _shader.Use();
            GL.BindVertexArray(_vertexArrayObject);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

            SwapBuffers();
        }

        private void OnUpdate(object sender, EventArgs e)
        {
            if (!Focused) return;

            HandleInput();
            Invalidate();
        }

        private void HandleInput()
        {
            // Handle keyboard inputs using System.Windows.Input
            if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.Escape))
            {
                System.Windows.Application.Current.Shutdown();
            }
        }

        private void OnResize(object sender, EventArgs e)
        {
            if (!Context.IsCurrent) MakeCurrent();
            GL.Viewport(0, 0, Width, Height);
        }

        public void Cleanup()
        {
            // Cleanup OpenGL resources
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            GL.DeleteBuffer(_vertexBufferObject);
            GL.DeleteVertexArray(_vertexArrayObject);
        }
    }
}
