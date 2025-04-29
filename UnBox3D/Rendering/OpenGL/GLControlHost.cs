using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.GLControl;
using UnBox3D.Utils;
using UnBox3D.Controls.States;
using UnBox3D.Controls;

namespace UnBox3D.Rendering.OpenGL
{
    public interface IGLControlHost : IDisposable
    {
        void Invalidate();
        int GetWidth();
        int GetHeight();
        void Render();
        void Cleanup();
    }

    public enum RenderMode
    {
        Wireframe,
        Solid,
        Point
    }

    public class GLControlHost : GLControl, IGLControlHost
    {
        private readonly float[] _vertices =
        {
            // Position          Normal
            -0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f, // Front
             0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,
             0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,
             0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,
            -0.5f,  0.5f, -0.5f,  0.0f,  0.0f, -1.0f,
            -0.5f, -0.5f, -0.5f,  0.0f,  0.0f, -1.0f,

            -0.5f, -0.5f,  0.5f,  0.0f,  0.0f,  1.0f, // Back
             0.5f, -0.5f,  0.5f,  0.0f,  0.0f,  1.0f,
             0.5f,  0.5f,  0.5f,  0.0f,  0.0f,  1.0f,
             0.5f,  0.5f,  0.5f,  0.0f,  0.0f,  1.0f,
            -0.5f,  0.5f,  0.5f,  0.0f,  0.0f,  1.0f,
            -0.5f, -0.5f,  0.5f,  0.0f,  0.0f,  1.0f,

            -0.5f,  0.5f,  0.5f, -1.0f,  0.0f,  0.0f, // Left
            -0.5f,  0.5f, -0.5f, -1.0f,  0.0f,  0.0f,
            -0.5f, -0.5f, -0.5f, -1.0f,  0.0f,  0.0f,
            -0.5f, -0.5f, -0.5f, -1.0f,  0.0f,  0.0f,
            -0.5f, -0.5f,  0.5f, -1.0f,  0.0f,  0.0f,
            -0.5f,  0.5f,  0.5f, -1.0f,  0.0f,  0.0f,

             0.5f,  0.5f,  0.5f,  1.0f,  0.0f,  0.0f, // Right
             0.5f,  0.5f, -0.5f,  1.0f,  0.0f,  0.0f,
             0.5f, -0.5f, -0.5f,  1.0f,  0.0f,  0.0f,
             0.5f, -0.5f, -0.5f,  1.0f,  0.0f,  0.0f,
             0.5f, -0.5f,  0.5f,  1.0f,  0.0f,  0.0f,
             0.5f,  0.5f,  0.5f,  1.0f,  0.0f,  0.0f,

            -0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f, // Bottom
             0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,
             0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,
             0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,
            -0.5f, -0.5f,  0.5f,  0.0f, -1.0f,  0.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, -1.0f,  0.0f,

            -0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f, // Top
             0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f,
             0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,
             0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,
            -0.5f,  0.5f,  0.5f,  0.0f,  1.0f,  0.0f,
            -0.5f,  0.5f, -0.5f,  0.0f,  1.0f,  0.0f
        };

        private readonly Vector3 _lightPos = new(1.2f, 1.0f, 2.0f);

        private int _vertexBufferObject;
        private int _vaoModel;
        private int _vaoLamp;

        private Shader _lampShader;
        private Shader _lightingShader;

        private readonly ISettingsManager _settingsManager;
        private readonly ISceneManager _sceneManager;
        private readonly IRenderer _sceneRenderer;

        private ICamera _camera;
        private MouseController _mouseController;
        private RayCaster _rayCaster;
        private KeyboardController _keyboardController;

        private RenderMode currentRenderMode;
        private ShadingModel currentShadingModel;
        private Vector4 backgroundColor;

        public GLControlHost(ISceneManager sceneManager, IRenderer sceneRenderer, ISettingsManager settingsManager)
        {
            Dock = DockStyle.Fill;
            _sceneManager = sceneManager;
            _sceneRenderer = sceneRenderer;
            _settingsManager = settingsManager;

            Load += GlControl_Load;
            Paint += GlControl_Paint;
            Resize += GlControl_Resize;

            MouseDown += OnMouseDown;
            MouseMove += OnMouseMove;
            MouseUp += OnMouseUp;
            MouseWheel += OnMouseWheel;
        }

        public void Render() => Invalidate();

        public int GetWidth() => Width;

        public int GetHeight() => Height;

        public new void Invalidate() => base.Invalidate();

        public void Cleanup()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);
        }

        public void SetRenderMode(RenderMode mode) => currentRenderMode = mode;

        public void SetShadingMode(ShadingModel shadingModel) => currentShadingModel = shadingModel;

        private void LoadSettingsFromJson()
        {
            string bgColor = _settingsManager.GetSetting<string>(new RenderingSettings().GetKey(), RenderingSettings.BackgroundColor);
            string renderMode = _settingsManager.GetSetting<string>(new RenderingSettings().GetKey(), RenderingSettings.RenderMode).ToLower();

            SetBackgroundColor(bgColor);

            currentRenderMode = renderMode switch
            {
                "wireframe" => RenderMode.Wireframe,
                "solid" => RenderMode.Solid,
                "point" => RenderMode.Point,
                _ => RenderMode.Wireframe
            };
        }

        private void SetBackgroundColor(string color)
        {
            color = color.ToLower().Replace(" ", "");
            BackgroundColors.TryGetBackgroundColor(color, out backgroundColor);
        }

        private void GlControl_Load(object sender, EventArgs e)
        {
            LoadSettingsFromJson();
            GL.ClearColor(backgroundColor.X, backgroundColor.Y, backgroundColor.Z, 1.0f);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);
            GL.Disable(EnableCap.CullFace);

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            _lightingShader = ShaderManager.LightingShader;
            _lampShader = ShaderManager.LampShader;

            SetupModelVAO();
            SetupLampVAO();

            _camera = new Camera(new Vector3(0, 0, 5), GetWidth() / (float)GetHeight());
            _rayCaster = new RayCaster(this, _camera);
            _mouseController = new MouseController(_settingsManager, _camera, new DefaultState(_sceneManager, this, _camera, _rayCaster), _rayCaster, this);
            _keyboardController = new KeyboardController(_camera);
        }

        private void SetupModelVAO()
        {
            _vaoModel = GL.GenVertexArray();
            GL.BindVertexArray(_vaoModel);

            var posLoc = _lightingShader.GetAttribLocation("aPos");
            var normLoc = _lightingShader.GetAttribLocation("aNormal");

            GL.EnableVertexAttribArray(posLoc);
            GL.VertexAttribPointer(posLoc, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            GL.EnableVertexAttribArray(normLoc);
            GL.VertexAttribPointer(normLoc, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
        }

        private void SetupLampVAO()
        {
            _vaoLamp = GL.GenVertexArray();
            GL.BindVertexArray(_vaoLamp);

            var posLoc = _lampShader.GetAttribLocation("aPos");
            GL.EnableVertexAttribArray(posLoc);
            GL.VertexAttribPointer(posLoc, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
        }

        private void GlControl_Paint(object sender, PaintEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _sceneRenderer.RenderScene(_camera, _lightingShader);

            _lampShader.Use();
            GL.BindVertexArray(_vaoLamp);

            Matrix4 model = Matrix4.CreateScale(0.2f) * Matrix4.CreateTranslation(_lightPos);
            _lampShader.SetMatrix4("model", model);
            _lampShader.SetMatrix4("view", _camera.GetViewMatrix());
            _lampShader.SetMatrix4("projection", _camera.GetProjectionMatrix());

            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            SwapBuffers();
        }

        private void GlControl_Resize(object sender, EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            if (_camera != null)
                _camera.AspectRatio = (float)Width / Height;
        }

        private void OnMouseDown(object sender, MouseEventArgs e) => _mouseController?.OnMouseDown(sender, e);
        private void OnMouseMove(object sender, MouseEventArgs e) => _mouseController?.OnMouseMove(sender, e);
        private void OnMouseUp(object sender, MouseEventArgs e) => _mouseController?.OnMouseUp(sender, e);
        private void OnMouseWheel(object sender, MouseEventArgs e) => _mouseController?.OnMouseWheel(sender, e);
    }
}
