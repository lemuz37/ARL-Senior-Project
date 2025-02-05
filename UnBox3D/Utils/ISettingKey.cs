namespace UnBox3D.Utils
{
    public interface ISettingKey
    {
        string GetKey();
    }
    public class AppSettings : ISettingKey
    {
        public static readonly string SplashScreenDuration = "SplashScreenDuration";
        public static readonly string ExportDirectory = "ExportDirectory";

        public string GetKey()
        {
            return "AppSettings";
        }
    }

    public class AssimpSettings : ISettingKey
    {
        public static readonly string Export = "Export";
        public static readonly string Import = "Import";
        public static readonly string EnableTriangulation = "EnableTriangulation";
        public static readonly string JoinIdenticalVertices = "JoinIdenticalVertices";
        public static readonly string RemoveComponents = "RemoveComponents";
        public static readonly string SplitLargeMeshes = "SplitLargeMeshes";
        public static readonly string OptimizeMeshes = "OptimizeMeshes";
        public static readonly string FindDegenerates = "FindDegenerates";
        public static readonly string FindInvalidData = "FindInvalidData";
        public static readonly string IgnoreInvalidData = "IgnoreInvalidData";

        public string GetKey()
        {
            return "AssimpSettings";
        }
    }

    public class RenderingSettings : ISettingKey
    {
        public static readonly string BackgroundColor = "BackgroundColor";
        public static readonly string MeshColor = "DefaultMeshColor";
        public static readonly string MeshHighlightColor = "MeshHighlightColor";
        public static readonly string RenderMode = "RenderMode";
        public static readonly string ShadingModel = "ShadingModel";
        public static readonly string LightingEnabled = "LightingEnabled";
        public static readonly string ShadowsEnabled = "ShadowsEnabled";
        public static readonly string CameraFOV = "CameraFOV";

        public string GetKey()
        {
            return "RenderingSettings";
        }
    }

    public class UISettings : ISettingKey
    {
        public static readonly string ToolStripPosition = "ToolStripPosition";
        public static readonly string CameraYawSensitivity = "CameraYawSensitivity";
        public static readonly string CameraPitchSensitivity = "CameraPitchSensitivity";
        public static readonly string CameraPanSensitivity = "CameraPanSensitivity";
        public static readonly string MeshRotationSensitivity = "MeshRotationSensitivity";
        public static readonly string MeshMoveSensitivity = "MeshMoveSensitivity";
        public static readonly string ZoomSensitivity = "ZoomSensitivity";

        public string GetKey()
        {
            return "UISettings";
        }
    }

    public class UnitsSettings : ISettingKey
    {
        public static readonly string DefaultUnit = "DefaultUnit";
        public static readonly string UseMetricSystem = "UseMetricSystem";

        public string GetKey()
        {
            return "UnitsSettings";
        }
    }

    public class WindowSettings : ISettingKey
    {
        public static readonly string Fullscreen = "Fullscreen";
        public static readonly string Height = "Height";
        public static readonly string Width = "Width";

        public string GetKey()
        {
            return "WindowSettings";
        }
    }
}