using UnBox3D.Utils;
using OpenTK.Graphics.OpenGL;
using System.Collections.ObjectModel;
using UnBox3D.Rendering.OpenGL;
using OpenTK.Mathematics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Assimp.Unmanaged;

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
                Vector3 lightPos = new Vector3(1.2f, 1.0f, 2.0f);
                shader.Use();

                shader.SetMatrix4("view", camera.GetViewMatrix());
                shader.SetMatrix4("projection", camera.GetProjectionMatrix());
                shader.SetVector3("lightColor", new Vector3(1.0f, 1.0f, 1.0f));
                shader.SetVector3("lightPos", lightPos);
                shader.SetVector3("viewPos", camera.Position);

                foreach (var appMesh in meshes)
                {
                    GL.BindVertexArray(appMesh.GetVAO());
                    Matrix4 model = Matrix4.CreateFromQuaternion(appMesh.GetTransform());
                    shader.SetMatrix4("model", model);
                    shader.SetVector3("objectColor", appMesh.GetColor());
                    GL.DrawElements(PrimitiveType.Triangles, appMesh.GetIndices().Length, DrawElementsType.UnsignedInt, 0);
                }
                GL.BindVertexArray(0);
            }
        }
    }
}
