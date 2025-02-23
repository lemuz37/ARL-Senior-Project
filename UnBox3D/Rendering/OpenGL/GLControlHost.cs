using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.GLControl;
using System.Windows.Input;
using System.Windows.Media;
using Assimp.Unmanaged;

namespace UnBox3D.Rendering.OpenGL
{
    public interface IGLControlHost : IDisposable
    {
        void Invalidate();
        int GetWidth();
        int GetHeight();
        void Cleanup();
        System.Drawing.Point PointToScreen(System.Drawing.Point point);

        event EventHandler<System.Windows.Input.MouseEventArgs> MouseDown;
        event EventHandler<System.Windows.Input.MouseEventArgs> MouseMove;
        event EventHandler<System.Windows.Input.MouseEventArgs> MouseUp;
        event EventHandler<System.Windows.Input.MouseWheelEventArgs> MouseWheel;
    }

    public class GLControlHost : GLControl, IGLControlHost
    {


        private readonly float[] _vertices =
        {
             // Position          Normal
            -0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f, // Front face
             0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,
             0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,
             0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,
            -0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,
            -0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,

            -0.5f, -0.5f,  0.5f,  0.0f,  0.0f,  1.0f, // Back face
             0.5f, -0.5f,  0.5f,  0.0f,  0.0f,  1.0f,
             0.5f,  0.5f,  0.5f,  0.0f,  0.0f,  1.0f,
             0.5f,  0.5f,  0.5f,  0.0f,  0.0f,  1.0f,
            -0.5f,  0.5f,  0.5f,  0.0f,  0.0f,  1.0f,
            -0.5f, -0.5f,  0.5f,  0.0f,  0.0f,  1.0f,

            -0.5f,  0.5f,  0.5f, -1.0f,  0.0f,  0.0f, // Left face
            -0.5f,  0.5f, -0.5f, -1.0f,  0.0f,  0.0f,
            -0.5f, -0.5f, -0.5f, -1.0f,  0.0f,  0.0f,
            -0.5f, -0.5f, -0.5f, -1.0f,  0.0f,  0.0f,
            -0.5f, -0.5f,  0.5f, -1.0f,  0.0f,  0.0f,
            -0.5f,  0.5f,  0.5f, -1.0f,  0.0f,  0.0f,

             0.5f,  0.5f,  0.5f,  1.0f,  0.0f,  0.0f, // Right face
             0.5f,  0.5f, -0.5f,  1.0f,  0.0f,  0.0f,
             0.5f, -0.5f, -0.5f,  1.0f,  0.0f,  0.0f,
             0.5f, -0.5f, -0.5f,  1.0f,  0.0f,  0.0f,
             0.5f, -0.5f,  0.5f,  1.0f,  0.0f,  0.0f,
             0.5f,  0.5f,  0.5f,  1.0f,  0.0f,  0.0f,

            -0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f, // Bottom face
             0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,
             0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,
             0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,
            -0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,

            -0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f, // Top face
             0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f,
             0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,
             0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,
            -0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,
            -0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f
        };

        private readonly Vector3 _lightPos = new Vector3(1.2f, 1.0f, 2.0f);

        private int _vertexBufferObject;
        private int _vaoModel;
        private int _vaoLamp;

        private Shader _lampShader;
        private Shader _lightingShader;
        private ICamera _camera;

        private bool _firstMove = true;
        private Vector2 _lastMousePos;

        private readonly ISceneManager _sceneManager;
        private readonly IRenderer _sceneRenderer;

        public GLControlHost(ISceneManager sceneManager, IRenderer sceneRenderer)
        {
            _sceneManager = sceneManager;
            _sceneRenderer = sceneRenderer;

            Dock = DockStyle.Fill;
            Load += OnLoad;
            Paint += OnRender;
            Resize += OnResize;

            CompositionTarget.Rendering += OnRenderFrame;
        }

        public int GetWidth() => Width;
        public int GetHeight() => Height;

        private void OnRenderFrame(object sender, EventArgs e)
        {
            Invalidate();
        }

        public void Invalidate()
        {
            base.Invalidate();
        }

        public event EventHandler<System.Windows.Input.MouseEventArgs> MouseDown;
        public event EventHandler<System.Windows.Input.MouseEventArgs> MouseMove;
        public event EventHandler<System.Windows.Input.MouseEventArgs> MouseUp;
        public event EventHandler<System.Windows.Input.MouseWheelEventArgs> MouseWheel;

        private void OnLoad(object sender, EventArgs e) 
        {
            GL.ClearColor(1.0f, .9f, 1.0f, .80f);
            GL.Enable(EnableCap.DepthTest);

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            _lightingShader = new Shader("Rendering/OpenGL/Shaders/shader.vert", "Rendering/OpenGL/Shaders/lighting.frag");
            _lampShader = new Shader("Rendering/OpenGL/Shaders/shader.vert", "Rendering/OpenGL/Shaders/shader.frag");

            {
                _vaoModel = GL.GenVertexArray();
                GL.BindVertexArray(_vaoModel);

                var positionLocation = _lightingShader.GetAttribLocation("aPos");
                GL.EnableVertexAttribArray(positionLocation);
                // Remember to change the stride as we now have 6 floats per vertex
                GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

                // We now need to define the layout of the normal so the shader can use it
                var normalLocation = _lightingShader.GetAttribLocation("aNormal");
                GL.EnableVertexAttribArray(normalLocation);
                GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            }

            {
                _vaoLamp = GL.GenVertexArray();
                GL.BindVertexArray(_vaoLamp);

                var positionLocation = _lampShader.GetAttribLocation("aPos");
                GL.EnableVertexAttribArray(positionLocation);
                GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            }

            _camera = new Camera(Vector3.UnitZ * 5, GetWidth() / (float)GetHeight());
        }

        private void OnRender(object sender, PaintEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            HandleInput();
            _sceneRenderer.RenderScene(_sceneManager.GetMeshes(), _lightingShader, _camera);

            RenderLamp();
            SwapBuffers();
        }

        private void RenderLamp()
        {
            GL.BindVertexArray(_vaoLamp);
            _lampShader.Use();

            var lampMatrix = Matrix4.CreateScale(0.2f) * Matrix4.CreateTranslation(_lightPos);
            _lampShader.SetMatrix4("model", lampMatrix);
            _lampShader.SetMatrix4("view", _camera.GetViewMatrix());
            _lampShader.SetMatrix4("projection", _camera.GetProjectionMatrix());
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
        }

        private void OnResize(object sender, EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            if(_camera != null)
                _camera.AspectRatio = (float)Width / Height;
        }

        private void HandleInput()
        {
            if (Keyboard.IsKeyDown(Key.Escape))
            {
                System.Windows.Application.Current.Shutdown();
            }

            const float cameraSpeed = 1.5f;
            const float sensitivity = 0.2f;

            if (Keyboard.IsKeyDown(Key.W)) _camera.Position += _camera.Front * cameraSpeed;
            if (Keyboard.IsKeyDown(Key.S)) _camera.Position -= _camera.Front * cameraSpeed;
            if (Keyboard.IsKeyDown(Key.A)) _camera.Position -= _camera.Right * cameraSpeed;
            if (Keyboard.IsKeyDown(Key.D)) _camera.Position += _camera.Right * cameraSpeed;
        }

        public void Cleanup()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            GL.DeleteBuffer(_vertexBufferObject);
            GL.DeleteVertexArray(_vaoModel);
            GL.DeleteVertexArray(_vaoLamp);
        }
    }
}
