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
    public partial class MainWindow : Window
    {
        private static readonly string BlenderFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Blender");
        private static readonly string BlenderZipPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "blender.zip");
        private static readonly string BlenderDownloadUrl = "https://download.blender.org/release/Blender4.2/blender-4.2.0-windows-x64.zip";


        private IGLControlHost? _controlHost;
        private ILogger? _logger;

        public MainWindow()
        {
            InitializeComponent();



            Loaded += async (s, e) => await CheckAndInstallBlender();
            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;
        }

        private async Task CheckAndInstallBlender()
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
            ZipFile.ExtractToDirectory(BlenderZipPath, BlenderFolder);
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
