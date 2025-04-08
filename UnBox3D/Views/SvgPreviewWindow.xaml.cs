using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using DrawingBitmap = System.Drawing.Bitmap;
using DrawingImageFormat = System.Drawing.Imaging.ImageFormat;
using WpfImage = System.Windows.Controls.Image;
using WpfOrientation = System.Windows.Controls.Orientation;
using WpfButton = System.Windows.Controls.Button;
using WpfMessageBox = System.Windows.MessageBox;
using WpfHorizontalAlignment = System.Windows.HorizontalAlignment;

namespace UnBox3D.Views
{
    public partial class SvgPreviewWindow : Window
    {
        public bool UserConfirmed { get; private set; } = false;

        public SvgPreviewWindow(List<string> pngFiles)
        {
            InitializeComponent();
            SetupPreview(pngFiles);
        }

        private void SetupPreview(List<string> pngFiles)
        {
            Title = "Preview Exported Pages";
            Width = 1000;
            Height = 800;

            ScrollViewer scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            WrapPanel wrapPanel = new WrapPanel
            {
                Orientation = WpfOrientation.Horizontal,
                Margin = new Thickness(10)
            };

            foreach (var file in pngFiles)
            {
                try
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(file);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    WpfImage image = new WpfImage
                    {
                        Source = bitmap,
                        Margin = new Thickness(10),
                        Stretch = System.Windows.Media.Stretch.None
                    };

                    wrapPanel.Children.Add(image);
                }
                catch (Exception ex)
                {
                    WpfMessageBox.Show(
                        $"Failed to load preview image: {file}\n{ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            }

            WpfButton confirmButton = new WpfButton
            {
                Content = "Confirm Export",
                Height = 40,
                Margin = new Thickness(10),
                HorizontalAlignment = WpfHorizontalAlignment.Center
            };
            confirmButton.Click += (s, e) => { UserConfirmed = true; this.Close(); };

            WpfButton cancelButton = new WpfButton
            {
                Content = "Cancel",
                Height = 40,
                Margin = new Thickness(10),
                HorizontalAlignment = WpfHorizontalAlignment.Center
            };
            cancelButton.Click += (s, e) => { UserConfirmed = false; this.Close(); };

            StackPanel mainPanel = new StackPanel();
            scrollViewer.Content = wrapPanel;
            mainPanel.Children.Add(scrollViewer);
            mainPanel.Children.Add(confirmButton);
            mainPanel.Children.Add(cancelButton);

            Content = mainPanel;
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            UserConfirmed = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            UserConfirmed = false;
            this.Close();
        }
    }
}
