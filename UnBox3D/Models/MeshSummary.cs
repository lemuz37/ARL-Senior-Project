using CommunityToolkit.Mvvm.Input;
using UnBox3D.Rendering;

namespace UnBox3D.Models
{
    /// <summary>
    /// Represents a lightweight summary of a mesh object used for UI display.
    /// Keeps track of the mesh name, vertex count, and a reference to the full mesh.
    /// Helps avoid performance issues by not exposing all the heavy data to the view.
    /// </summary>
    public partial class MeshSummary
    {
        public string Name { get; set; }
        public int VertexCount { get; set; }
        public string Display => $"{Name} ({VertexCount} vertices)";
        public IAppMesh SourceMesh { get; set; }

        public MeshSummary(IAppMesh source)
        {
            SourceMesh = source;
            Name = source.Name;
            VertexCount = source.VertexCount;
        }

        // Mesh Simplification Commands
        [RelayCommand]
        private void SimplifyDecimation(IAppMesh mesh)
        {
            if (mesh is AppMesh appMesh)
            {
                // Ensure that the simplification (and subsequent GL calls) occur on the UI thread
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    appMesh.SimplifyDecimation();
                });
            }
        }

        [RelayCommand]
        private void SimplifyEdgeCollapse(IAppMesh mesh)
        {
            if (mesh is AppMesh appMesh)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    appMesh.SimplifyEdgeCollapse();
                });
            }
        }

        [RelayCommand]
        private void SimplifyAdaptiveDecimation(IAppMesh mesh)
        {
            if (mesh is AppMesh appMesh)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    appMesh.SimplifyAdaptiveDecimation();
                });
            }
        }
    }
}