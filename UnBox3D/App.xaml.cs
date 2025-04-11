using Microsoft.Extensions.DependencyInjection;
using OpenTK.Mathematics;
using System.IO;
using System.Windows;
using UnBox3D.Models;
using UnBox3D.Rendering;
using UnBox3D.Rendering.OpenGL;
using UnBox3D.Utils;
using UnBox3D.ViewModels;
using UnBox3D.Views;
using Application = System.Windows.Application;

namespace UnBox3D
{
    public partial class App : Application
    {
        private ServiceProvider? _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Configure the service provider (Dependency Injection container)
            _serviceProvider = ConfigureServices();

            // Resolve MainWindow and MainViewModel from the DI container
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            var mainViewModel = _serviceProvider.GetRequiredService<MainViewModel>();
            mainWindow.DataContext = mainViewModel;

            // Initialize MainWindow with required services and show it
            mainWindow.Initialize(
                _serviceProvider.GetRequiredService<IGLControlHost>(),
                _serviceProvider.GetRequiredService<ILogger>(),
                _serviceProvider.GetRequiredService<IBlenderInstaller>()
            );
            mainWindow.Show();
        }

        private ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            #region Core Utilities Registration
            services.AddSingleton<IFileSystem, FileSystem>();
            services.AddSingleton<ILogger, Logger>(provider =>
            {
                var fileSystem = provider.GetRequiredService<IFileSystem>();
                return new Logger(
                    fileSystem,
                    logDirectory: @"C:\\ProgramData\\UnBox3D\\Logs",
                    logFileName: "UnBox3D.log"
                );
            });
            services.AddSingleton<ISettingsManager, SettingsManager>();
            #endregion

            #region Rendering Services Registration
            services.AddSingleton<ISceneManager, SceneManager>();
            services.AddSingleton<IRenderer, SceneRenderer>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger>();
                var settings = provider.GetRequiredService<ISettingsManager>();
                var sceneManager = provider.GetRequiredService<ISceneManager>();
                return new SceneRenderer(logger, settings, sceneManager);
            });
            services.AddSingleton<IGLControlHost, GLControlHost>(provider =>
            {
                var sceneManager = provider.GetRequiredService<ISceneManager>();
                var sceneRenderer = provider.GetRequiredService<IRenderer>();
                var settingsManager = provider.GetRequiredService<ISettingsManager>();
                return new GLControlHost(sceneManager, sceneRenderer, settingsManager);
            });
            #endregion

            #region UI and ViewModel Registration
            services.AddSingleton<IBlenderInstaller, BlenderInstaller>(provider =>
            {
                var fileSystem = provider.GetRequiredService<IFileSystem>();
                return new BlenderInstaller(fileSystem);
            });
            services.AddSingleton<ModelExporter>(provider => {
                var settingsManager = provider.GetRequiredService<ISettingsManager>();
                return new ModelExporter(settingsManager);
            });


            services.AddSingleton<BlenderIntegration>();
            services.AddSingleton<MainWindow>();

            services.AddSingleton<MainViewModel>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger>();
                var settings = provider.GetRequiredService<ISettingsManager>();
                var sceneManager = provider.GetRequiredService<ISceneManager>();
                var fileSystem = provider.GetRequiredService<IFileSystem>();
                var blenderIntegration = provider.GetRequiredService<BlenderIntegration>();
                var blenderInstaller = provider.GetRequiredService<IBlenderInstaller>();
                var modelExporter = provider.GetRequiredService<ModelExporter>();
                return new MainViewModel(logger, settings, sceneManager, fileSystem, blenderIntegration, blenderInstaller, modelExporter);
            });
            #endregion

            return services.BuildServiceProvider();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // 1. Read the "CleanupExportOnExit" setting
            var settingsManager = _serviceProvider?.GetRequiredService<ISettingsManager>();
            if (settingsManager != null)
            {
                bool cleanupOnExit = settingsManager.GetSetting<bool>(
                    new AppSettings().GetKey(),
                    AppSettings.CleanupExportOnExit
                );

                // 2. If the user wants cleanup, do it
                if (cleanupOnExit)
                {
                    // Also fetch the export directory from settings
                    string? exportDir = settingsManager.GetSetting<string>(
                        new AppSettings().GetKey(),
                        AppSettings.ExportDirectory
                    );

                    // Fallback if it doesn't exist
                    if (string.IsNullOrWhiteSpace(exportDir) || !Directory.Exists(exportDir))
                    {
                        exportDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Export");
                    }

                    try
                    {
                        //Remove only .obj files
                        foreach (var file in Directory.GetFiles(exportDir, "*.obj"))
                        {
                            File.Delete(file);
                        }
                        foreach (var file in Directory.GetFiles(exportDir, "*.mtl"))
                        {
                            File.Delete(file);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to clean up export directory: {ex.Message}");
                    }
                }
            }
            // Clean up the service provider on exit
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}
