using UnBox3D.Utils;
using OpenTK.Graphics.OpenGL;
using System.Collections.ObjectModel;

namespace UnBox3D.Rendering
{
    public interface IRenderer
    {
        void RenderScene(ObservableCollection<IAppMesh> meshes, ICamera camera);
    }

    public class SceneRenderer : IRenderer
    {
        private readonly ILogger _logger;

        public SceneRenderer(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _logger.Info("Initializing SceneRenderer");
        }

        public void RenderScene(ObservableCollection<IAppMesh> meshes, ICamera camera)
        {
            if (meshes == null || meshes.Count == 0)
            {
                _logger.Warn("No meshes available for rendering.");
                return;
            }

            foreach (var appMesh in meshes)
            {
                _logger.Info($"Rendering mesh '{appMesh.GetName()}'.");

                GL.DrawArrays(PrimitiveType.Triangles, 0, appMesh.VertexCount);
            }
            GL.BindVertexArray(0);
        }
    }
}
