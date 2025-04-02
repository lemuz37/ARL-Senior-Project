using UnBox3D.Utils;
using OpenTK.Graphics.OpenGL;
using System.Collections.ObjectModel;
using UnBox3D.Rendering.OpenGL;
using OpenTK.Mathematics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace UnBox3D.Rendering
{
    public interface IRenderer
    {
        void RenderScene(ObservableCollection<IAppMesh> meshes, ICamera camera, Shader shader);
    }

    public class SceneRenderer : IRenderer
    {
        private readonly ILogger _logger;
        private readonly ISettingsManager _settingsManager;


        public SceneRenderer(ILogger logger, ISettingsManager settingsManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));

            _logger.Info("Initializing SceneRenderer");
        }

        public void RenderScene(ObservableCollection<IAppMesh> meshes, ICamera camera, Shader shader)
        {
            if (meshes == null || meshes.Count == 0)
            {
                _logger.Warn("No meshes available for rendering.");
                return;
            }
            else
            {
                foreach (var appMesh in meshes)
                {
                    Vector3 lightPos = new Vector3(1.2f, 1.0f, 2.0f);

                    _logger.Info($"Rendering mesh '{appMesh.GetName()}'.");
                    GL.BindVertexArray(appMesh.GetVAO());

                    shader.Use();
                    shader.SetMatrix4("model", Matrix4.Identity);
                    shader.SetMatrix4("view", camera.GetViewMatrix());
                    shader.SetMatrix4("projection", camera.GetProjectionMatrix());

                    shader.SetVector3("objectColor", new Vector3(1.0f, 0.5f, 0.31f));
                    shader.SetVector3("lightColor", new Vector3(1.0f, 1.0f, 1.0f));
                    shader.SetVector3("lightPos", lightPos);
                    shader.SetVector3("viewPos", camera.Position);

                    GL.DrawArrays(PrimitiveType.Triangles, 0, appMesh.VertexCount);
                }
                GL.BindVertexArray(0);
            }
        }
    }
}
