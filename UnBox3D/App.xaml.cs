using Microsoft.Extensions.DependencyInjection;
using OpenTK.Mathematics;
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
                return new SceneRenderer(logger, settings);
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
            services.AddSingleton<IBlenderInstaller, BlenderInstaller>();
            services.AddSingleton<BlenderIntegration>();
            services.AddSingleton<MainWindow>();

            services.AddSingleton<MainViewModel>(provider =>
            {
                var settings = provider.GetRequiredService<ISettingsManager>();
                var sceneManager = provider.GetRequiredService<ISceneManager>();
                var fileSystem = provider.GetRequiredService<IFileSystem>();
                var blenderIntegration = provider.GetRequiredService<BlenderIntegration>();
                var blenderInstaller = provider.GetRequiredService<IBlenderInstaller>();
                return new MainViewModel(settings, sceneManager, fileSystem, blenderIntegration, blenderInstaller);
            });
            #endregion

            return services.BuildServiceProvider();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Clean up the service provider on exit
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}
