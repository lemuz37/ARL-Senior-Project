using System;
using System.Windows;
using UnBox3D.Rendering.OpenGL;
using UnBox3D.Utils;

namespace UnBox3D.Views
{
    public partial class MainWindow : Window
    {
        private IGLControlHost? _controlHost;
        private ILogger? _logger;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;
        }

        // Inject dependencies
        public void Initialize(IGLControlHost controlHost, ILogger logger)
        {
            _controlHost = controlHost ?? throw new ArgumentNullException(nameof(controlHost));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _logger?.Info("MainWindow loaded. Initializing OpenGL...");

                // Attach GLControlHost to WindowsFormsHost
                 openGLHost.Child = (Control)_controlHost;

                _logger?.Info("GLControlHost successfully attached to WindowsFormsHost.");
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error initializing OpenGL: {ex.Message}");
                System.Windows.MessageBox.Show($"Error initializing OpenGL: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            try
            {
                _logger?.Info("MainWindow is closing. Performing cleanup...");
                _controlHost?.Cleanup();
                (_controlHost as IDisposable)?.Dispose();
                _logger?.Info("Cleanup completed successfully.");
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error during cleanup: {ex.Message}");
            }
        }
    }
}
