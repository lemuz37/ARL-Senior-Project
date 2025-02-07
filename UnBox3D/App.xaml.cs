using Microsoft.Extensions.DependencyInjection;
using OpenTK.Mathematics;
using System.Windows;
using UnBox3D.Models;
using UnBox3D.Rendering;
using UnBox3D.Rendering.OpenGL;
using UnBox3D.Utils;
using UnBox3D.ViewModels;
using UnBox3D.Views;

namespace UnBox3D
{
    public partial class App : System.Windows.Application
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

            // Assign DataContext to MainWindow
            mainWindow.DataContext = mainViewModel;

            // Resolve MainWindow and Show
            mainWindow.Initialize(
                _serviceProvider.GetRequiredService<IGLControlHost>(),
                _serviceProvider.GetRequiredService<ILogger>()
            );
            mainWindow.Show();
        }

        private ServiceProvider ConfigureServices()
        {
            var serviceCollection = new ServiceCollection();

            // Register MainWindow and MainViewModel
            serviceCollection.AddSingleton<MainWindow>();
            serviceCollection.AddSingleton<MainViewModel>();

            // Register other services
            serviceCollection.AddSingleton<IFileSystem, FileSystem>();
            serviceCollection.AddSingleton<ILogger, Logger>(provider =>
            {
                var fileSystem = provider.GetRequiredService<IFileSystem>();
                return new Logger(fileSystem, logDirectory: @"C:\ProgramData\UnBox3D\Logs", logFileName: "UnBox3D.log");
            });
            serviceCollection.AddSingleton<ISettingsManager, SettingsManager>();
            serviceCollection.AddSingleton<ICommandHistory, CommandHistory>();

            // Register OpenGL and rendering services
            serviceCollection.AddSingleton<IGLControlHost, GLControlHost>();
            serviceCollection.AddSingleton<IRenderer, SceneRenderer>();
            serviceCollection.AddSingleton<ISceneManager, SceneManager>();
            serviceCollection.AddSingleton<IRayCaster, RayCaster>();
            serviceCollection.AddSingleton<ICamera, Camera>(provider =>
            {
                var settingsManager = provider.GetRequiredService<ISettingsManager>();
                var glControlHost = provider.GetRequiredService<IGLControlHost>();
                return new Camera(new Vector3(0, 0, 3), glControlHost.GetWidth() / glControlHost.GetHeight());
            });



            return serviceCollection.BuildServiceProvider();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Dispose the service provider when the application exits
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}
