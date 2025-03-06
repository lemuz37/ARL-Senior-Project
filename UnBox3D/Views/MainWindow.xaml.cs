using Microsoft.Win32;
using System.IO;
using System.Net.Http;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows;
using UnBox3D.Rendering.OpenGL;
using UnBox3D.Utils;

namespace UnBox3D.Views
{
    public partial class MainWindow : Window, IBlenderInstaller
    {
        private static readonly string BlenderFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Blender");
        private static readonly string BlenderZipPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "blender.zip");
        private static readonly string BlenderDownloadUrl = "https://download.blender.org/release/Blender4.2/blender-4.2.0-windows-x64.zip";

        // Cache the installation task, so multiple calls will await the first call
        private Task? _blenderInstallTask;
        private IGLControlHost? _controlHost;
        private ILogger? _logger;

        public MainWindow()
        {
            InitializeComponent();

            
            
            Loaded += async (s, e) => await CheckAndInstallBlender();
            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;
        }

        public Task CheckAndInstallBlender()
        {
            // If theres no blender task start installation
            if (_blenderInstallTask == null)
            {
                _blenderInstallTask = CheckAndInstallBlenderInternal();
            }
            return _blenderInstallTask;
        }

        private async Task CheckAndInstallBlenderInternal()
        {
            if (!Directory.Exists(BlenderFolder))
            {
                Debug.WriteLine("Blender 4.2 is not installed. Downloading now...");
                await DownloadAndExtractBlender();
            }
            else
            {
                Debug.WriteLine("Blender 4.2 is already installed.");
            }
        }

        private static async Task DownloadAndExtractBlender()
        {
            using HttpClient client = new HttpClient();
            var response = await client.GetAsync(BlenderDownloadUrl);
            response.EnsureSuccessStatusCode();

            byte[] data = await response.Content.ReadAsByteArrayAsync();
            await File.WriteAllBytesAsync(BlenderZipPath, data);

            Debug.WriteLine("Download complete. Extracting Blender...");

            // Ensure the target directory exists
            if (!Directory.Exists(BlenderFolder))
            {
                Directory.CreateDirectory(BlenderFolder);
            }

            ZipFile.ExtractToDirectory(BlenderZipPath, BlenderFolder, overwriteFiles: true);
            File.Delete(BlenderZipPath);

            Debug.WriteLine("Blender installation completed.");
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
