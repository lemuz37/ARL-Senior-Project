using System.Windows;
using System.Windows.Forms.Integration;
using UnBox3D.OpenGL;

namespace UnBox3D.Views
{
    public partial class MainWindow : Window
    {
        private GLControlHost _glControlHost;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize the custom GLControlHost
            _glControlHost = new GLControlHost();

            // Attach it to the WindowsFormsHost
            WindowsFormsHost host = openGLHost;
            host.Child = _glControlHost;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            // Perform cleanup
            _glControlHost?.Cleanup();
        }
    }
}
