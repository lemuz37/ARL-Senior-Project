using OpenTK.Mathematics;
using g4;
using System.Collections.ObjectModel;
using UnBox3D.Utils;

namespace UnBox3D.Rendering
{
    public interface ISceneManager
    {
        ObservableCollection<IAppMesh> GetMeshes();
        void AddMesh(IAppMesh mesh);
        void DeleteMesh(IAppMesh mesh);
        void ClearScene();
        void RemoveSmallMeshes(float threshold);
        void ReplaceMesh(IAppMesh oldMesh, IAppMesh newMesh);
        Vector3 GetMeshCenter(DMesh3 mesh);
        Vector3 GetMeshDimensions(DMesh3 mesh);
        List<AppMesh> LoadBoundingBoxes();
        void ScaleAllMeshesToTargetDimension(float targetSize, Axis axis);
    }

    public enum Axis
    {
        X,
        Y,
        Z
    }

    public class SceneManager : ISceneManager
    {
        private ObservableCollection<IAppMesh> _sceneMeshes;
        private readonly ILogger _logger;

        public SceneManager(ILogger logger)
        {
            _sceneMeshes = new ObservableCollection<IAppMesh>();
            _logger = logger;
        }

        public ObservableCollection<IAppMesh> GetMeshes() => _sceneMeshes;

        public void AddMesh(IAppMesh mesh)
        {
            if (mesh != null)
            {
                _sceneMeshes.Add(mesh);
                _logger?.Info($"Added mesh: {mesh.Name}");
            }
        }

        public void DeleteMesh(IAppMesh mesh)
        {
            if (mesh == null) return;

            if (_sceneMeshes.Contains(mesh))
            {
                _sceneMeshes.Remove(mesh);
                _logger?.Info($"Deleted mesh: {mesh.Name}");
            }
        }

        public void ClearScene()
        {
            _sceneMeshes.Clear();
            _logger?.Info("Scene cleared of all meshes.");
        }

        public void ReplaceMesh(IAppMesh oldMesh, IAppMesh newMesh)
        {
            if (_sceneMeshes.Contains(oldMesh))
            {
                int index = _sceneMeshes.IndexOf(oldMesh);
                _sceneMeshes[index] = newMesh;
                _logger?.Info($"Replaced mesh: {oldMesh.Name} with {newMesh.Name}");
            }
            else
            {
                _sceneMeshes.Add(newMesh);
                _logger?.Info($"Added replacement mesh: {newMesh.Name}");
            }
        }

        public void RemoveSmallMeshes(float thresholdRatio)
        {
            if (_sceneMeshes.Count == 0)
                return;

            float maxDimension = _sceneMeshes
                .Select(mesh => GetMeshDimensions(mesh.GetG4Mesh()))
                .Select(dim => Math.Max(dim.X, Math.Max(dim.Y, dim.Z)))
                .Max();

            float threshold = thresholdRatio * maxDimension;

            var meshesToRemove = _sceneMeshes
                .Where(mesh =>
                {
                    Vector3 dims = GetMeshDimensions(mesh.GetG4Mesh());
                    return dims.X < threshold || dims.Y < threshold || dims.Z < threshold;
                })
                .ToList();

            foreach (var mesh in meshesToRemove)
            {
                _sceneMeshes.Remove(mesh);
                _logger?.Info($"Removed small mesh: {mesh.Name}");
            }
        }

        public Vector3 GetMeshCenter(DMesh3 mesh)
        {
            if (mesh.VertexCount > 0)
            {
                Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

                for (int i = 0; i < mesh.VertexCount; i++)
                {
                    g4.Vector3d vertex = mesh.GetVertex(i);
                    Vector3 vertexVec = new Vector3((float)vertex.x, (float)vertex.y, (float)vertex.z);

                    min = Vector3.ComponentMin(min, vertexVec);
                    max = Vector3.ComponentMax(max, vertexVec);
                }

                return (min + max) * 0.5f;
            }

            return Vector3.Zero;
        }

        public Vector3 GetMeshDimensions(DMesh3 mesh)
        {
            if (mesh.VertexCount > 0)
            {
                float largestXDimension = 0.0f;
                float largestYDimension = 0.0f;
                float largestZDimension = 0.0f;

                Vector3 meshCenter = GetMeshCenter(mesh);

                for (int i = 0; i < mesh.VertexCount; i++)
                {
                    g4.Vector3d vertex = mesh.GetVertex(i);
                    Vector3 vertexVec = new Vector3((float)vertex.x, (float)vertex.y, (float)vertex.z);

                    largestXDimension = Math.Max(largestXDimension, Math.Abs(vertexVec.X - meshCenter.X));
                    largestYDimension = Math.Max(largestYDimension, Math.Abs(vertexVec.Y - meshCenter.Y));
                    largestZDimension = Math.Max(largestZDimension, Math.Abs(vertexVec.Z - meshCenter.Z));
                }

                return new Vector3(largestXDimension * 2, largestYDimension * 2, largestZDimension * 2);
            }

            return Vector3.Zero;
        }

        public List<AppMesh> LoadBoundingBoxes()
        {
            var originalMeshes = _sceneMeshes.ToList();
            var boxMeshes = new List<AppMesh>();

            foreach (IAppMesh mesh in originalMeshes)
            {
                DMesh3 geomMesh = mesh.GetG4Mesh();
                Vector3 meshCenter = GetMeshCenter(geomMesh);
                Vector3 meshDimensions = GetMeshDimensions(geomMesh);

                AppMesh boxMesh = GeometryGenerator.CreateBox(meshCenter, meshDimensions.X, meshDimensions.Y, meshDimensions.Z, mesh.Name);
                boxMeshes.Add(boxMesh);

                _sceneMeshes.Add(boxMesh);
                _sceneMeshes.Remove(mesh);
                _logger?.Info($"Replaced mesh {mesh.Name} with its bounding box.");
            }

            return boxMeshes;
        }

        public void ScaleAllMeshesToTargetDimension(float targetSize, Axis axis)
        {
            var meshes = _sceneMeshes.ToList();
            if (meshes.Count == 0)
            {
                _logger?.Warn("No meshes to scale.");
                return;
            }

            AxisAlignedBox3d modelBounds = AxisAlignedBox3d.Empty;
            var meshPairs = new List<(DMesh3 dmesh, IAppMesh originalMesh)>();

            foreach (var mesh in meshes)
            {
                DMesh3 dmesh = new DMesh3();
                var source = mesh.GetG4Mesh();

                foreach (int vi in source.VertexIndices())
                {
                    var v = source.GetVertex(vi);
                    dmesh.AppendVertex(new g4.Vector3d(v.x, v.y, v.z));
                }

                foreach (int ti in source.TriangleIndices())
                {
                    var tri = source.GetTriangle(ti);
                    dmesh.AppendTriangle(tri.a, tri.b, tri.c);
                }

                modelBounds.Contain(dmesh.CachedBounds);
                meshPairs.Add((dmesh, mesh));
            }

            double maxDim = axis switch
            {
                Axis.X => modelBounds.Width,
                Axis.Y => modelBounds.Height,
                Axis.Z => modelBounds.Depth,
                _ => modelBounds.MaxDim
            };

            if (maxDim == 0)
            {
                _logger?.Warn("Max dimension is zero. Cannot scale.");
                return;
            }

            double scaleFactor = targetSize / maxDim;
            _logger?.Info($"Scaling all meshes by factor {scaleFactor:F4} on axis {axis} to fit target size {targetSize}.");

            foreach (var (dmesh, _) in meshPairs)
            {
                MeshTransforms.Scale(dmesh, scaleFactor);
            }

            AxisAlignedBox3d scaledBounds = AxisAlignedBox3d.Empty;
            foreach (var (dmesh, _) in meshPairs)
            {
                scaledBounds.Contain(dmesh.CachedBounds);
            }

            g4.Vector3d center = scaledBounds.Center;

            foreach (var (dmesh, originalMesh) in meshPairs)
            {
                MeshTransforms.Translate(dmesh, -center);

                var assimpMesh = originalMesh.GetAssimpMesh();

                for (int vi = 0; vi < dmesh.VertexCount; vi++)
                {
                    var v = dmesh.GetVertex(vi);
                    assimpMesh.Vertices[vi] = new Assimp.Vector3D((float)v.x, (float)v.y, (float)v.z);
                }

                var newMesh = new AppMesh(dmesh, assimpMesh);
                newMesh.SetColor(originalMesh.GetColor());

                ReplaceMesh(originalMesh, newMesh);
            }

            _logger?.Info("All meshes scaled and recentered successfully.");
        }
    }
}
