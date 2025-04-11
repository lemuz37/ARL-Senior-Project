using Microsoft.Extensions.DependencyInjection;
using OpenTK.Mathematics;
using System.ServiceProcess;
using System.Windows;
using UnBox3D.Controls;
using UnBox3D.Controls.States;
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
            services.AddSingleton<ICommandHistory, CommandHistory>();
            services.AddSingleton<IState, DefaultState>(provider =>
            {
                var sceneManager = provider.GetRequiredService<ISceneManager>();
                var glHost = provider.GetRequiredService<GLControlHost>();
                var camera = provider.GetRequiredService<ICamera>();
                var rayCaster = provider.GetRequiredService<IRayCaster>();
                return new DefaultState(sceneManager, glHost, camera, rayCaster);
            });
            #endregion

            #region Rendering Services Registration
            services.AddSingleton<IRayCaster, RayCaster>();
            services.AddSingleton<ICamera, Camera>(provider =>
            {
                Vector3 defaultPos = new Vector3(0, 0, 0);
                float defaultAspectRatio = 16f / 9f;
                return new Camera(defaultPos, defaultAspectRatio);
            });
            services.AddSingleton<ISceneManager, SceneManager>();
            services.AddSingleton<IRenderer, SceneRenderer>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger>();
                var settings = provider.GetRequiredService<ISettingsManager>();
                return new SceneRenderer(logger, settings);
            });
            services.AddSingleton<GLControlHost>(provider =>
            {
                var sceneManager = provider.GetRequiredService<ISceneManager>();
                var sceneRenderer = provider.GetRequiredService<IRenderer>();
                var settingsManager = provider.GetRequiredService<ISettingsManager>();
                return new GLControlHost(sceneManager, sceneRenderer, settingsManager);
            });
            services.AddSingleton<IGLControlHost>(provider => provider.GetRequiredService<GLControlHost>());
            #endregion

            #region UI and ViewModel Registration
            services.AddSingleton<IBlenderInstaller, BlenderInstaller>(provider =>
            {
                var fileSystem = provider.GetRequiredService<IFileSystem>();
                return new BlenderInstaller(fileSystem);
            });

            services.AddSingleton<MouseController>(provider =>
            {
                var settingsManger = provider.GetRequiredService<ISettingsManager>();
                var camera = provider.GetRequiredService<ICamera>();
                var neutralState = provider.GetRequiredService<IState>();
                var rayCaster = provider.GetRequiredService<IRayCaster>();
                var glControlHost = provider.GetRequiredService<GLControlHost>();

                return new MouseController(settingsManger, camera, neutralState, rayCaster, glControlHost);
            });
            services.AddSingleton<BlenderIntegration>();
            services.AddSingleton<MainWindow>();

            services.AddSingleton<MainViewModel>(provider =>
            {
                var settings = provider.GetRequiredService<ISettingsManager>();
                var sceneManager = provider.GetRequiredService<ISceneManager>();
                var fileSystem = provider.GetRequiredService<IFileSystem>();
                var blenderIntegration = provider.GetRequiredService<BlenderIntegration>();
                var blenderInstaller = provider.GetRequiredService<IBlenderInstaller>();
                var mouseController = provider.GetRequiredService<MouseController>();
                var camera = provider.GetRequiredService<ICamera>();
                var glControlHost = provider.GetRequiredService<IGLControlHost>();
                var commandHistory = provider.GetRequiredService<ICommandHistory>();

                return new MainViewModel(settings, sceneManager, fileSystem, blenderIntegration, blenderInstaller, mouseController, glControlHost, camera, commandHistory);
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
