using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UnBox3D.Utils
{
    public interface ISettingsManager 
    {
        void SaveSettings();
        T GetSetting<T>(T defaultValue = default, params string[] keys);
        void UpdateSetting(object updateValue, string mainSettingKey, string subSettingKey);
        void UpdateSetting(object updateValue, string mainSettingKey, string subSettingKey, string settingToUpdateKey);
    }
    public class SettingsManager : ISettingsManager
    {
        private readonly IFileSystem _fileSystem;
        private readonly ILogger _logger;
        private readonly string _settingsFilePath;
        private readonly object _lock = new object();
        private Dictionary<string, object> settings;
        private object dict;

        public SettingsManager(IFileSystem fileSystem, ILogger logger)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            string settingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "UnBox3D", "Settings");
            _settingsFilePath = Path.Combine(settingsDirectory, "settings.json");

            if (!_fileSystem.DoesDirectoryExists(settingsDirectory))
            {
                _logger.Info($"Settings directory not found. Creating directory: {settingsDirectory}");
                _fileSystem.CreateDirectory(settingsDirectory);
            }

            LoadSettings();
        }

        private void LoadSettings()
        {
            lock (_lock)
            {
                try
                {
                    _logger.Info("Attempting to load settings.");
                    if (_fileSystem.DoesFileExists(_settingsFilePath))
                    {
                        _logger.Info($"Settings file found. Loading settings from: {_settingsFilePath}");
                        // Load existing settings from file.
                        var json = _fileSystem.ReadFile(_settingsFilePath);
                        settings = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                        _logger.Info("Settings loaded successfully.");
                    }
                    else
                    {
                        _logger.Warn("Settings file not found. Initializing with default settings.");
                        settings = GetDefaultSettings();
                        SaveSettings();
                        _logger.Info("Default settings initialized and saved.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to load settings. Error: {ex.Message}");
                    throw;
                }
            }
        }


        /// <summary>
        /// Provides default values for application settings.
        /// This is the main location for developers to set or update default values for settings used throughout the application.
        /// Each setting is organized into categories that match the structure of the settings.json file.
        /// To change a default value, simply modify the corresponding entry in this dictionary.
        /// </summary>
        private Dictionary<string, object> GetDefaultSettings()
        {
            var appSettings = new AppSettings();
            var assimpSettings = new AssimpSettings();
            var renderingSettings = new RenderingSettings();
            var uiSettings = new UISettings();
            var unitsSettings = new UnitsSettings();
            var windowSettings = new WindowSettings();

            return new Dictionary<string, object>
    {
        { appSettings.GetKey(), new Dictionary<string, object>
            {
                { AppSettings.SplashScreenDuration, appSettings.DefaultSplashScreenDuration },
                { AppSettings.ExportDirectory, appSettings.DefaultExportDirectory }
            }
        },

        { assimpSettings.GetKey(), new Dictionary<string, object>
            {
                { AssimpSettings.Export, new Dictionary<string, object>() },
                { AssimpSettings.Import, new Dictionary<string, object>
                    {
                        { AssimpSettings.EnableTriangulation, assimpSettings.DefaultEnableTriangulation },
                        { AssimpSettings.JoinIdenticalVertices, assimpSettings.DefaultJoinIdenticalVertices },
                        { AssimpSettings.RemoveComponents, assimpSettings.DefaultRemoveComponents },
                        { AssimpSettings.SplitLargeMeshes, assimpSettings.DefaultSplitLargeMeshes },
                        { AssimpSettings.OptimizeMeshes, assimpSettings.DefaultOptimizeMeshes },
                        { AssimpSettings.FindDegenerates, assimpSettings.DefaultFindDegenerates },
                        { AssimpSettings.FindInvalidData, assimpSettings.DefaultFindInvalidData },
                        { AssimpSettings.IgnoreInvalidData, assimpSettings.DefaultIgnoreInvalidData }
                    }
                }
            }
        },

        { renderingSettings.GetKey(), new Dictionary<string, object>
            {
                { RenderingSettings.BackgroundColor, renderingSettings.DefaultBackgroundColor },
                { RenderingSettings.MeshColor, renderingSettings.DefaultMeshColor },
                { RenderingSettings.MeshHighlightColor, renderingSettings.DefaultMeshHighlightColor },
                { RenderingSettings.RenderMode, renderingSettings.DefaultRenderMode },
                { RenderingSettings.ShadingModel, renderingSettings.DefaultShadingModel },
                { RenderingSettings.LightingEnabled, renderingSettings.DefaultLightingEnabled },
                { RenderingSettings.ShadowsEnabled, renderingSettings.DefaultShadowsEnabled }
            }
        },

        { uiSettings.GetKey(), new Dictionary<string, object>
            {
                { UISettings.ToolStripPosition, uiSettings.DefaultToolStripPosition },
                { UISettings.CameraYawSensitivity, uiSettings.DefaultCameraYawSensitivity },
                { UISettings.CameraPitchSensitivity, uiSettings.DefaultCameraPitchSensitivity },
                { UISettings.CameraPanSensitivity, uiSettings.DefaultCameraPanSensitivity },
                { UISettings.MeshRotationSensitivity, uiSettings.DefaultMeshRotationSensitivity },
                { UISettings.MeshMoveSensitivity, uiSettings.DefaultMeshMoveSensitivity },
                { UISettings.ZoomSensitivity, uiSettings.DefaultZoomSensitivity }
            }
        },

        { unitsSettings.GetKey(), new Dictionary<string, object>
            {
                { UnitsSettings.DefaultUnit, unitsSettings.DefaultDefaultUnit },
                { UnitsSettings.UseMetricSystem, unitsSettings.DefaultUseMetricSystem }
            }
        },

        { windowSettings.GetKey(), new Dictionary<string, object>
            {
                { WindowSettings.Fullscreen, windowSettings.DefaultFullscreen },
                { WindowSettings.Height, windowSettings.DefaultHeight },
                { WindowSettings.Width, windowSettings.DefaultWidth }
            }
        }
    };
        }

        public void SaveSettings()
        {
            lock (_lock)
            {
                var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                _fileSystem.WriteToFile(_settingsFilePath, json);
            }
        }

        public T GetSetting<T>(T defaultValue = default, params string[] keys)
        {
            lock (_lock)
            {
                _logger.Info($"Attempting to retrieve setting for keys: {string.Join(" -> ", keys)}");

                object currentSetting = settings;

                // Traverse the nested dictionaries based on the provided keys
                foreach (var key in keys)
                {
                    if (currentSetting is Dictionary<string, object> dict && dict.ContainsKey(key))
                    {
                        _logger.Info($"Found key '{key}' in Dictionary.");
                        currentSetting = dict[key];
                    }
                    else if (currentSetting is JObject jObject)
                    {
                        var settingDict = jObject.ToObject<Dictionary<string, object>>();
                        if (settingDict != null && settingDict.ContainsKey(key))
                        {
                            _logger.Info($"Found key '{key}' in JObject.");
                            currentSetting = settingDict[key];
                        }
                        else
                        {
                            _logger.Warn($"Key '{key}' not found in JObject. Returning default value.");
                            return defaultValue;
                        }
                    }
                    else
                    {
                        _logger.Warn($"Key '{key}' not found. Returning default value.");
                        return defaultValue;
                    }
                }

                if (currentSetting is T value)
                {
                    _logger.Info($"Successfully retrieved setting for keys: {string.Join(" -> ", keys)}. Value: {value}");
                    return value;
                }

                try
                {
                    _logger.Info($"Attempting to deserialize setting for keys: {string.Join(" -> ", keys)}.");
                    return JsonConvert.DeserializeObject<T>(currentSetting.ToString());
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to deserialize setting for keys: {string.Join(" -> ", keys)}. Returning default value. Error: {ex.Message}");
                    return defaultValue;
                }
            }
        }

        // Update a setting. Thread-safe with lock. Supports two levels of nested dictionaries.
        // Two-Level Solution (e.g., "RenderingSettings" -> "BackgroundColor")
        // mainSettingKey The immediate parent key (e.g., "AppSettings"). The top-level category (e.g., "AppSettings" or "RenderingSettings")
        // subSettingKey The actual setting to update (e.g., "SplashScreenDuration"). The subcategory (e.g., "SplashScreenDuration")
        public void UpdateSetting(object updateValue, string mainSettingKey, string subSettingKey)
        {
            lock (_lock)
            {
                try
                {
                    LoadSettings();
                    _logger.Info($"Attempting to update setting: {mainSettingKey} -> {subSettingKey} with value: {updateValue}");

                    if (settings.ContainsKey(mainSettingKey))
                    {
                        settings.TryGetValue(mainSettingKey, out dict);
                        if (dict is Dictionary<string, object>)
                        {
                            Dictionary<string, object> mainSettings = (Dictionary<string, object>)dict;
                            mainSettings[subSettingKey] = updateValue;
                            settings[mainSettingKey] = mainSettings;
                            _logger.Info($"Updated {mainSettingKey} -> {subSettingKey} with value: {updateValue}.");
                            SaveSettings();
                        }
                        else
                        {
                            _logger.Error($"Key {subSettingKey} not found in {mainSettingKey}. Aborting update.");
                            throw new Exception();
                        }
                    }
                    else
                    {
                        _logger.Warn($"Major setting {mainSettingKey} not found. Aborting update.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to update setting for keys: {string.Join(" -> ", mainSettingKey, subSettingKey)} with value: {updateValue}. Error: {ex.Message}");
                }
            }
        }
        // Update a setting. Thread-safe with lock. Supports three levels of nested dictionaries.
        // Three-Level Solution (e.g., "AssimpSettings" -> "Import" -> "EnableTriangulation")
        // mainSettingKey: The immediate parent key (e.g., ""AssimpSettings""). The top-level category (e.g., ""AssimpSettings"" or "RenderingSettings")
        // subSettingKey: The nested dictionary within the parent key (e.g., "Import"). The subcategory (e.g., "Import")
        // settingToUpdateKey: The actual setting to update (e.g., "EnableTriangulation").

        public void UpdateSetting(object updateValue, string mainSettingKey, string subSettingKey, string settingToUpdateKey)
        {
            lock (_lock)
            {
                try
                {
                    // Check if the parent setting (e.g., "AssimpSettings") exists
                    if (settings.ContainsKey(mainSettingKey) && settings[mainSettingKey] is Dictionary<string, object> mainSettings)
                    {
                        // Check if the parent setting (e.g., "Import" or "Export") exists
                        if (mainSettings.ContainsKey(subSettingKey) && mainSettings[subSettingKey] is Dictionary<string, object> subSettings)
                        {
                            if (subSettings.ContainsKey(settingToUpdateKey))
                            {
                                // Update the sub-setting value
                                subSettings[settingToUpdateKey] = updateValue;

                                // Update the parent setting with the modified sub-settings
                                mainSettings[subSettingKey] = subSettings;

                                // Update the top-level settings with the modified main settings
                                settings[mainSettingKey] = mainSettings;

                                _logger.Info($"Updated {mainSettingKey} -> {subSettingKey} -> {settingToUpdateKey} with value: {updateValue}.");
                                SaveSettings();
                            }
                            else
                            {
                                _logger.Warn($"Key {settingToUpdateKey} not found in {subSettingKey}. Aborting update.");
                            }
                        }
                        else
                        {
                            _logger.Warn($"Key {subSettingKey} not found in {mainSettings}. Aborting update.");
                        }
                    }
                    else
                    {
                        _logger.Warn($"Major setting {mainSettingKey} not found. Aborting update.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to update setting for keys: {string.Join(" -> ", mainSettingKey, subSettingKey, settingToUpdateKey)} with value: {updateValue}. Error: {ex.Message}");
                }
            }
        }
    }
}