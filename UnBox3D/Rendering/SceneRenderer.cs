using UnBox3D.Utils;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using UnBox3D.Rendering.OpenGL;
using System.Collections.ObjectModel;

namespace UnBox3D.Rendering
{
    public interface IRenderer
    {
        void RenderScene(ObservableCollection<IAppMesh> meshes, Shader lightningShader, ICamera camera);
    }

    public class SceneRenderer : IRenderer
    {
        private readonly ILogger _logger;

        public SceneRenderer(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _logger.Info("Initializing SceneRenderer");
        }

        public void RenderScene(ObservableCollection<IAppMesh> meshes, Shader lightingShader, ICamera camera)
        {
            if (meshes == null || meshes.Count == 0)
            {
                _logger.Warn("No meshes available for rendering.");
                return;
            }

            Vector3 lightPos = new Vector3(1.2f, 1.0f, 2.0f);

            lightingShader.Use();
            lightingShader.SetMatrix4("view", camera.GetViewMatrix());
            lightingShader.SetMatrix4("projection", camera.GetProjectionMatrix());

            foreach (var appMesh in meshes)
            {
                _logger.Info($"Rendering mesh '{appMesh.GetName()}'.");
                lightingShader.SetVector3("objectColor", appMesh.GetColor());
                lightingShader.SetVector3("lightColor", Vector3.One);
                lightingShader.SetVector3("lightPos", lightPos);
                lightingShader.SetVector3("viewPos", camera.Position);
                GL.BindVertexArray(appMesh.GetVAO());
                GL.DrawArrays(PrimitiveType.Triangles, 0, appMesh.VertexCount);
            }
            GL.BindVertexArray(0);
        }
    }
}
