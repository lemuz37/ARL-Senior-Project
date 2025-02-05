using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UnBox3D.Utils
{
    public class SettingsManager
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
            return new Dictionary<string, object>
            {
                { new AppSettings().GetKey(), new Dictionary<string, object>
                    {
                        { AppSettings.SplashScreenDuration, 3.0f },
                        { AppSettings.ExportDirectory, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "UnBox3D", "Export") }
                    }
                },

                { new AssimpSettings().GetKey(), new Dictionary<string, object>
                    {
                        { AssimpSettings.Export, new Dictionary<string, object>
                            {

                            }
                        },
                        { AssimpSettings.Import, new Dictionary<string, object>
                            {
                                { AssimpSettings.EnableTriangulation, true },
                                { AssimpSettings.JoinIdenticalVertices, true },
                                { AssimpSettings.RemoveComponents, false },
                                { AssimpSettings.SplitLargeMeshes, true },
                                { AssimpSettings.OptimizeMeshes, true },
                                { AssimpSettings.FindDegenerates, true },
                                { AssimpSettings.FindInvalidData, true },
                                { AssimpSettings.IgnoreInvalidData, false }
                            }
                        }
                    }
                },

                { new RenderingSettings().GetKey(), new Dictionary<string, object>
                    {
                        { RenderingSettings.BackgroundColor, "lightgrey" },
                        { RenderingSettings.MeshColor, "red" },
                        { RenderingSettings.MeshHighlightColor, "cyan" },
                        { RenderingSettings.RenderMode, "wireframe" },
                        { RenderingSettings.ShadingModel, "smooth" },
                        { RenderingSettings.LightingEnabled, true },
                        { RenderingSettings.ShadowsEnabled, false }
                    }
                },

                { new UISettings().GetKey(), new Dictionary<string, object>
                    {
                        { UISettings.ToolStripPosition, "top" },
                        { UISettings.CameraYawSensitivity, 0.2f },
                        { UISettings.CameraPitchSensitivity, 0.2f },
                        { UISettings.CameraPanSensitivity, 1000.0f },
                        { UISettings.MeshRotationSensitivity, 0.2f },
                        { UISettings.MeshMoveSensitivity, 0.2f },
                        { UISettings.ZoomSensitivity, 1.0f }
                    }
                },

                { new UnitsSettings().GetKey(), new Dictionary<string, object>
                    {
                        { UnitsSettings.DefaultUnit, "Feet" },
                        { UnitsSettings.UseMetricSystem, false }
                    }
                },

                { new WindowSettings().GetKey(), new Dictionary<string, object>
                    {
                        { WindowSettings.Fullscreen, false },
                        { WindowSettings.Height, 720 },
                        { WindowSettings.Width, 1280 }
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

        // Update a setting. Thread-safe with lock. Supports up to three levels of nested dictionaries.
        public void UpdateSetting(object value, params string[] keys)
        {
            lock (_lock)
            {
                try
                {
                    string keyToUpdateValue = keys[keys.Length - 1];  // The actual setting to update (e.g., "BackgroundColor")
                    string keyParentSetting = keys[keys.Length - 2];  // The immediate parent key (e.g., "RenderingSettings")
                    string mainSetting = keys[0];                     // The top-level category (e.g., "AssimpSettings" or "RenderingSettings")
                    string subSetting = keys[1];                      // The potential subcategory (e.g., "Import")
                    LoadSettings();


                    _logger.Info($"Attempting to update setting: {mainSetting} -> {keyParentSetting} -> {keyToUpdateValue} with value: {value}");

                    // Two-Level Solution (e.g., "RenderingSettings" -> "BackgroundColor")
                    if (keyToUpdateValue == subSetting)
                    {
                        if (settings.ContainsKey(mainSetting))
                        {
                            settings.TryGetValue(mainSetting, out dict);
                            if (dict is Dictionary<string, object>)
                            {
                                Dictionary<string, object> mainSettings = (Dictionary<string, object>)dict;
                                mainSettings[keyToUpdateValue] = value;
                                settings[mainSetting] = mainSettings;
                                _logger.Info($"Updated {mainSetting} -> {keyToUpdateValue} with value: {value}.");
                                SaveSettings();
                            }
                            else
                            {
                                _logger.Warn($"Key {keyToUpdateValue} not found in {mainSetting}. Aborting update.");
                            }
                        }
                        else
                        {
                            _logger.Warn($"Major setting {mainSetting} not found. Aborting update.");
                        }
                    }
                    // Three-Level Solution (e.g., "AssimpSettings" -> "Import" -> "EnableTriangulation")
                    else
                    {
                        if (settings.ContainsKey(mainSetting) && settings[mainSetting] is Dictionary<string, object> mainSettings)
                        {
                            // Check if the parent setting (e.g., "Import" or "Export") exists
                            if (mainSettings.ContainsKey(keyParentSetting) && mainSettings[keyParentSetting] is Dictionary<string, object> subSettings)
                            {
                                if (subSettings.ContainsKey(keyToUpdateValue))
                                {
                                    // Update the sub-setting value
                                    subSettings[keyToUpdateValue] = value;

                                    // Update the parent setting with the modified sub-settings
                                    mainSettings[keyParentSetting] = subSettings;

                                    // Update the top-level settings with the modified main settings
                                    settings[mainSetting] = mainSettings;

                                    _logger.Info($"Updated {mainSetting} -> {keyParentSetting} -> {keyToUpdateValue} with value: {value}.");
                                    SaveSettings();
                                }
                                else
                                {
                                    _logger.Warn($"Key {keyToUpdateValue} not found in {keyParentSetting}. Aborting update.");
                                }
                            }
                            else
                            {
                                _logger.Warn($"Key {keyParentSetting} not found in {mainSetting}. Aborting update.");
                            }
                        }
                        else
                        {
                            _logger.Warn($"Major setting {mainSetting} not found. Aborting update.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to update setting for keys: {string.Join(" -> ", keys)} with value: {value}. Error: {ex.Message}");
                }
            }
        }
    }
}