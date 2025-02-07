using OpenTK.Graphics.OpenGL;
using UnBox3D.Utils;
using OpenTK.Mathematics;

namespace UnBox3D.Rendering
{
    public interface IRenderer
    {
        void RenderScene(List<IAppMesh> meshes);
    }

    public class SceneRenderer : IRenderer
    {
        private readonly ILogger _logger;

        public SceneRenderer(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _logger.Info("Initializing SceneRenderer");
        }

        public void RenderScene(List<IAppMesh> meshes)
        {
            if (meshes == null || meshes.Count == 0)
            {
                _logger.Warn("No meshes available for rendering.");
                return;
            }

            foreach (var appMesh in meshes)
            {
                Vector3 color = appMesh.GetColor();
                RenderMesh(appMesh, color);
            }
        }

        private void RenderMesh(IAppMesh appMesh, Vector3 color)
        {
            _logger.Info($"Rendering mesh '{appMesh.GetName()}'.");

            GL.Color3(color.X, color.Y, color.Z);
            GL.Begin(PrimitiveType.Triangles);

            foreach (Vector3 vertex in appMesh.GetVertices())
            {
                GL.Vertex3(vertex.X, vertex.Z, vertex.Y);
            }

            GL.End();
        }
    }
}
