using System.Windows;
using OpenTK.Graphics.OpenGL;
using OpenTK.GLControl;
using System.Windows.Controls;

namespace UnBox3D
{
    public partial class MainWindow : Window
    {
        private GLControl _glControl;

        public MainWindow()
        {
            InitializeComponent();

            // Initialize GLControl with default settings
            var glControlSettings = new GLControlSettings();
            _glControl = new GLControl(glControlSettings);

            // Attach event handlers for GLControl
            _glControl.Load += GlControl_Load;
            _glControl.Paint += GlControl_Paint;

            // Embed GLControl into the WindowsFormsHost
            openTKWindow.Child = _glControl; // openTKWindow is from XAML
        }

        private void ImportFileMenu_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "3D Models|*.obj;*.fbx;*.stl",
                Title = "Import 3D Model"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                // Load the 3D model
            }
        }

        private void SaveFileMenu_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "3D Models|*.obj;*.fbx;*.stl",
                Title = "Save 3D Model"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;
                // Save the 3D model
            }
        }

        private void ExitMenu_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void WireframeMenu_Click(object sender, RoutedEventArgs e)
        {
            // Toggle Wireframe Mode
            var menuItem = sender as MenuItem;
            bool isWireframe = menuItem.IsChecked;
            // Apply wireframe mode based on `isWireframe`
        }

        private void ShadedMenu_Click(object sender, RoutedEventArgs e)
        {
            // Toggle Shaded Mode
            var menuItem = sender as MenuItem;
            bool isShaded = menuItem.IsChecked;
            // Apply shaded mode based on `isShaded`
        }

        private void GraphicsLowMenu_Click(object sender, RoutedEventArgs e)
        {
            // Set graphics quality to Low
        }

        private void GraphicsMediumMenu_Click(object sender, RoutedEventArgs e)
        {
            // Set graphics quality to Medium
        }

        private void GraphicsHighMenu_Click(object sender, RoutedEventArgs e)
        {
            // Set graphics quality to High
        }

        private void AboutMenu_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("UnBox3D Application\nVersion 1.0\nDeveloped by [Your Name/Company]", "About Us");
        }

        private void GlControl_Load(object? sender, EventArgs e)
        {
            GL.ClearColor(System.Drawing.Color.Black); // Set the background color
        }

        private void GlControl_Paint(object? sender, System.Windows.Forms.PaintEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Add rendering logic here

            _glControl.SwapBuffers(); // Swap the front and back buffers
        }


        private void ApplySettings(string backgroundColor, string meshColor)
        {
            // Example: Change background color of the rendering area
            if (backgroundColor != null)
            {
                switch (backgroundColor)
                {
                    case "Black":
                        openTKWindow.Background = System.Windows.Media.Brushes.Black;
                        break;
                    case "White":
                        openTKWindow.Background = System.Windows.Media.Brushes.White;
                        break;
                    case "Gray":
                        openTKWindow.Background = System.Windows.Media.Brushes.Gray;
                        break;
                }
            }

            // Example: Change mesh color (implementation depends on your 3D rendering logic)
            if (meshColor != null)
            {
                // Pass this value to your 3D rendering logic to change mesh color
            }
        }
    }
}