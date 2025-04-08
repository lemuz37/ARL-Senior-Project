using Microsoft.Win32;
using System.IO;
using System.Net.Http;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows;
using UnBox3D.Rendering.OpenGL;
using UnBox3D.Utils;
using System.Windows.Input;
using TextBox = System.Windows.Controls.TextBox;

namespace UnBox3D.Views
{
    public partial class MainWindow : Window, IBlenderInstaller
    {
        private static readonly string BlenderFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Blender");
        private static readonly string BlenderExecutable = Path.Combine(BlenderFolder, "blender-4.2.0-windows-x64", "blender.exe");
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

        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;

            // Only allow digits and one decimal
            if (!char.IsDigit(e.Text[0]) && e.Text != ".")
            {
                e.Handled = true;
                return;
            }

            if (e.Text == "." && textBox.Text.Contains("."))
            {
                e.Handled = true;
                return;
            }
        }

        private void NumericTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Allow navigation, deletion and control keys
            if (e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.Left ||
                e.Key == Key.Right || e.Key == Key.Tab)
            {
                return;
            }

            // Handle clipboard operations
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.V)
            {
                var textBox = sender as TextBox;

                if (System.Windows.Clipboard.ContainsText())
                {
                    string clipboardText = System.Windows.Clipboard.GetText();

                    if (!IsValidDecimalInput(clipboardText))
                    {
                        e.Handled = true;
                        return;
                    }

                    string resultText = textBox.Text.Substring(0, textBox.SelectionStart) +
                                        clipboardText +
                                        textBox.Text.Substring(textBox.SelectionStart + textBox.SelectionLength);

                    if (resultText.Count(c => c == '.') > 1)
                    {
                        e.Handled = true;
                        return;
                    }
                }
            }
        }

        private bool IsValidDecimalInput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            bool hasDecimal = false;

            foreach (char c in input)
            {
                if (c == '.')
                {
                    if (hasDecimal)
                        return false;
                    hasDecimal = true;
                }
                else if (!char.IsDigit(c))
                {
                    return false;
                }
            }

            return true;
        }

        private void NumericTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            int cursorPosition = textBox.SelectionStart;
            string originalText = textBox.Text;

            if (string.IsNullOrEmpty(textBox.Text) || textBox.Text == ".")
                return;

            if (float.TryParse(textBox.Text, out float value) &&
                textBox.DataContext is ViewModels.MainViewModel viewModel)
            {
                if (textBox.Name.Contains("Width"))
                    viewModel.PageWidth = value;
                else if (textBox.Name.Contains("Height"))
                    viewModel.PageHeight = value;
            }

            if (textBox.Text != originalText)
            {
                int charsAdded = textBox.Text.Length - originalText.Length;
                cursorPosition += charsAdded > 0 ? charsAdded : 0;
            }

            textBox.SelectionStart = cursorPosition;
        }

        private void NumericTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;

            if (string.IsNullOrWhiteSpace(textBox.Text) || textBox.Text == ".")
            {
                textBox.Text = "0";

                if (textBox.DataContext is ViewModels.MainViewModel viewModel)
                {
                    if (textBox.Name.Contains("Width"))
                        viewModel.PageWidth = 0;
                    else if (textBox.Name.Contains("Height"))
                        viewModel.PageHeight = 0;
                }
            }
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
            if (!Directory.Exists(BlenderFolder) || !File.Exists(BlenderExecutable))
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

                StartUpdateLoop();
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

        private async void StartUpdateLoop() 
        {
            bool _isRunning = true;

            while (_isRunning) 
            {
                Debug.WriteLine("Updating...");

                _controlHost.Render();

                await Task.Delay(16); // 60 FPS
            }
        }
    }
}
