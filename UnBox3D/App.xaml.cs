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

            // Configure the service provider (DI container)
            _serviceProvider = ConfigureServices();

            // Resolve MainWindow and MainViewModel
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            var mainViewModel = _serviceProvider.GetRequiredService<MainViewModel>();
            mainWindow.DataContext = mainViewModel;

            // Initialize and show MainWindow
            mainWindow.Initialize(
                _serviceProvider.GetRequiredService<IGLControlHost>(),
                _serviceProvider.GetRequiredService<ILogger>()
            );
            mainWindow.Show();
        }

        private ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Register core utilities
            services.AddSingleton<IFileSystem, FileSystem>();
            services.AddSingleton<ILogger, Logger>(provider =>
            {
                var fileSystem = provider.GetRequiredService<IFileSystem>();
                return new Logger(fileSystem,
                    logDirectory: @"C:\\ProgramData\\UnBox3D\\Logs",
                    logFileName: "UnBox3D.log");
            });
            services.AddSingleton<ISettingsManager, SettingsManager>();

            // Register OpenGL and rendering services
            services.AddSingleton<ISceneManager, SceneManager>();
            services.AddSingleton<IRenderer, SceneRenderer>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger>();
                return new SceneRenderer(logger);
            });
            services.AddSingleton<IGLControlHost, GLControlHost>(provider =>
            {
                var sceneManager = provider.GetRequiredService<ISceneManager>();
                var sceneRenderer = provider.GetRequiredService<IRenderer>();
                return new GLControlHost(sceneManager, sceneRenderer);
            });

            // Register MainWindow and MainViewModel
            services.AddSingleton<MainWindow>();
            services.AddSingleton<MainViewModel>(provider =>
            {
                var settings = provider.GetRequiredService<ISettingsManager>();
                var sceneManager = provider.GetRequiredService<ISceneManager>();
                return new MainViewModel(settings, sceneManager);
            });

            return services.BuildServiceProvider();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Dispose the service provider when the application exits
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}
