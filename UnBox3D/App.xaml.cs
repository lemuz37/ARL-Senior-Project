using Microsoft.Extensions.DependencyInjection;
using OpenTK.Mathematics;
using System.IO;
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

            _serviceProvider = ConfigureServices();

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            var mainViewModel = _serviceProvider.GetRequiredService<MainViewModel>();
            mainWindow.DataContext = mainViewModel;

            mainWindow.Initialize(
                _serviceProvider.GetRequiredService<IGLControlHost>(),
                _serviceProvider.GetRequiredService<ILogger>(),
                _serviceProvider.GetRequiredService<IBlenderInstaller>()
            );

            mainWindow.Show();

            var settingsWindow = _serviceProvider.GetRequiredService<SettingsWindow>();
            settingsWindow.Initialize(
                _serviceProvider.GetRequiredService<ILogger>(),
                _serviceProvider.GetRequiredService<ISettingsManager>()
            );
            settingsWindow.Owner = mainWindow;
            settingsWindow.Hide();
        }

        private ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            RegisterCoreServices(services);
            RegisterRenderingServices(services);
            RegisterUIAndViewModels(services);
            return services.BuildServiceProvider();
        }

        private void RegisterCoreServices(IServiceCollection services)
        {
            services.AddSingleton<IFileSystem, FileSystem>();
            services.AddSingleton<ILogger>(CreateLogger);
            services.AddSingleton<ICommandHistory, CommandHistory>();
            services.AddSingleton<IState, DefaultState>(provider =>
            {
                return new DefaultState(
                    provider.GetRequiredService<ISceneManager>(),
                    provider.GetRequiredService<IGLControlHost>(),
                    provider.GetRequiredService<ICamera>(),
                    provider.GetRequiredService<IRayCaster>()
                );
            });
            services.AddSingleton<ISettingsManager, SettingsManager>();
            services.AddSingleton<SVGEditor>(provider => new SVGEditor(
                provider.GetRequiredService<ILogger>()));
        }

        private void RegisterRenderingServices(IServiceCollection services)
        {
            services.AddSingleton<ISceneManager, SceneManager>();
            services.AddSingleton<IRayCaster, RayCaster>();
            services.AddSingleton<ICamera>(provider => new Camera(new Vector3(0, 0, 0), 16f / 9f));
            services.AddSingleton<IRenderer>(provider => new SceneRenderer(
                provider.GetRequiredService<ILogger>(),
                provider.GetRequiredService<ISettingsManager>(),
                provider.GetRequiredService<ISceneManager>()
            ));
            services.AddSingleton<GLControlHost>(provider => new GLControlHost(
                provider.GetRequiredService<ISceneManager>(),
                provider.GetRequiredService<IRenderer>(),
                provider.GetRequiredService<ISettingsManager>()
            ));
            services.AddSingleton<IGLControlHost>(provider => provider.GetRequiredService<GLControlHost>());
        }

        private void RegisterUIAndViewModels(IServiceCollection services)
        {
            services.AddSingleton<IBlenderInstaller>(provider =>
                new BlenderInstaller(provider.GetRequiredService<IFileSystem>(), provider.GetRequiredService<ILogger>()));
            services.AddSingleton<ModelExporter>(provider =>
                new ModelExporter(provider.GetRequiredService<ISettingsManager>(), provider.GetRequiredService<ILogger>()));
            services.AddSingleton<MouseController>(provider => new MouseController(
                provider.GetRequiredService<ISettingsManager>(),
                provider.GetRequiredService<ICamera>(),
                provider.GetRequiredService<IState>(),
                provider.GetRequiredService<IRayCaster>(),
                provider.GetRequiredService<GLControlHost>()
            ));

            services.AddSingleton<BlenderIntegration>();
            services.AddSingleton<SettingsWindow>();
            services.AddSingleton<MainWindow>();

            services.AddSingleton<MainViewModel>(provider => new MainViewModel(
                provider.GetRequiredService<ILogger>(),
                provider.GetRequiredService<ISettingsManager>(),
                provider.GetRequiredService<ISceneManager>(),
                provider.GetRequiredService<IFileSystem>(),
                provider.GetRequiredService<BlenderIntegration>(),
                provider.GetRequiredService<IBlenderInstaller>(),
                provider.GetRequiredService<ModelExporter>(),
                provider.GetRequiredService<ICommandHistory>(),
                provider.GetRequiredService<SVGEditor>()
            ));
        }

        private static ILogger CreateLogger(IServiceProvider provider)
        {
            var fileSystem = provider.GetRequiredService<IFileSystem>();
            return new Logger(fileSystem, @"C:\\ProgramData\\UnBox3D\\Logs", "UnBox3D.log");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            CleanupExportDirectory();
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }

        private void CleanupExportDirectory()
        {
            var settingsManager = _serviceProvider?.GetRequiredService<ISettingsManager>();
            if (settingsManager == null) return;

            bool cleanupOnExit = settingsManager.GetSetting<bool>(
                new AppSettings().GetKey(),
                AppSettings.CleanupExportOnExit
            );

            if (!cleanupOnExit) return;

            string? exportDir = settingsManager.GetSetting<string>(
                new AppSettings().GetKey(),
                AppSettings.ExportDirectory
            );

            if (string.IsNullOrWhiteSpace(exportDir) || !Directory.Exists(exportDir))
            {
                exportDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Export");
            }

            try
            {
                foreach (var file in Directory.GetFiles(exportDir, "*.obj")) File.Delete(file);
                foreach (var file in Directory.GetFiles(exportDir, "*.mtl")) File.Delete(file);
            }
            catch (Exception ex)
            {
                _serviceProvider?.GetService<ILogger>()?.Info($"Failed to clean up export directory: {ex.Message}");
            }
        }
    }
}