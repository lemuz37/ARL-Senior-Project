using OpenTK.Mathematics;

namespace UnBox3D.Rendering.Geometry
{
    public interface IGeometryFactory
    {
        AppMesh CreateShape();
    }

    public class BoxFactory : IGeometryFactory
    {
        private Vector3 _center;
        private float _width, _height, _depth;

        public BoxFactory(Vector3 center, float width, float height, float depth)
        {
            _center = center;
            _width = width;
            _height = height;
            _depth = depth;
        }

        public AppMesh CreateShape()
        {
            return GeometryGenerator.CreateBox(_center, _width, _height, _depth);
        }
    }

    public class CylinderFactory : IGeometryFactory
    {
        private Vector3 _center;
        private float _radius, _height;
        private int _segments;

        public CylinderFactory(Vector3 center, float radius, float height, int segments)
        {
            _center = center;
            _radius = radius;
            _height = height;
            _segments = segments;
        }

        public AppMesh CreateShape()
        {
            return GeometryGenerator.CreateCylinder(_center, _radius, _height, _segments);
        }
    }
}
