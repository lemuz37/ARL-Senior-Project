using System.Configuration;
using System.Data;
using System.Windows;
using UnBox3D.Utils;

namespace UnBox3D
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public static Logger AppLogger { get; private set; }
        public static SettingsManager AppSettingsManager { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initilize logger
            var fileSystem = new FileSystem();
            AppLogger = new Logger(fileSystem, logDirectory: @"C:\ProgramData\UnBox3D\Logs", logFileName: "UnBox3D.log");

            // Initialize SettingsManager
            AppSettingsManager = new SettingsManager(fileSystem, AppLogger);

            AppLogger.Info("Application started.");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            AppLogger.Info("Application exiting.");
            base.OnExit(e);
        }
    }
}
