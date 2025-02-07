using OpenTK.Graphics.OpenGL;
using UnBox3D.Utils;

namespace UnBox3D.Rendering
{
    public class GridPlaneRenderer
    {
        public void DrawTransparentXZPlaneWithGrid(float gridSize = 100000.0f, int gridLines = 100, float transparency = 0.3f)
        {
            // Enable blending to draw transparent objects
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Disable(EnableCap.DepthTest);

            // Light gray plane with transparency
            GL.Color4(0.8f, 0.8f, 0.8f, transparency);

            // Draw the transparent XZ-plane
            GL.Begin(PrimitiveType.Quads);
            GL.Vertex3(-gridSize, 0, -gridSize);  // Bottom left
            GL.Vertex3(gridSize, 0, -gridSize);   // Bottom right
            GL.Vertex3(gridSize, 0, gridSize);    // Top right
            GL.Vertex3(-gridSize, 0, gridSize);   // Top left
            GL.End();

            // Draw the grid lines over the XZ-plane
            GL.Color3(Colors.Black);
            GL.LineWidth(1.0f);

            GL.Begin(PrimitiveType.Lines);

            // Vertical grid lines (parallel to Z-axis)
            for (int i = -gridLines; i <= gridLines; i++)
            {
                float x = i * (gridSize / gridLines);
                GL.Vertex3(x, 0, -gridSize);  // Line from back to front
                GL.Vertex3(x, 0, gridSize);
            }

            // Horizontal grid lines (parallel to X-axis)
            for (int i = -gridLines; i <= gridLines; i++)
            {
                float z = i * (gridSize / gridLines);
                GL.Vertex3(-gridSize, 0, z);  // Line from left to right
                GL.Vertex3(gridSize, 0, z);
            }

            GL.End();

            // Make the X and Z axes thicker and colored
            GL.LineWidth(3.0f);  // Thicker line width for the X and Z axes
            GL.Begin(PrimitiveType.Lines);

            // Positive X-axis
            GL.Color3(Colors.Green);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(gridSize, 0, 0);

            // Negative X-axis
            GL.Color3(Colors.Blue);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(-gridSize, 0, 0);

            // Positive Z-axis
            GL.Color3(Colors.Yellow);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 0, gridSize);

            // Negative Z-axis
            GL.Color3(Colors.Black);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 0, -gridSize);

            GL.End();

            // Restore settings
            GL.LineWidth(1.0f);   // Reset line width to default
            GL.Enable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Blend);
        }
    }
}