using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

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

            System.Windows.Controls.ScrollViewer scrollViewer = new System.Windows.Controls.ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            System.Windows.Controls.WrapPanel wrapPanel = new System.Windows.Controls.WrapPanel
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal,
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

                    System.Windows.Controls.Image image = new System.Windows.Controls.Image
                    {
                        Source = bitmap,
                        Margin = new Thickness(10),
                        Stretch = System.Windows.Media.Stretch.None
                    };

                    wrapPanel.Children.Add(image);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(
                        $"Failed to load preview image: {file}\n{ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            }

            System.Windows.Controls.Button confirmButton = new System.Windows.Controls.Button
            {
                Content = "Confirm Export",
                Height = 40,
                Margin = new Thickness(10),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center
            };
            confirmButton.Click += (s, e) => { UserConfirmed = true; this.Close(); };

            System.Windows.Controls.StackPanel mainPanel = new System.Windows.Controls.StackPanel();
            scrollViewer.Content = wrapPanel;
            mainPanel.Children.Add(scrollViewer);
            mainPanel.Children.Add(confirmButton);

            Content = mainPanel;
        }
    }
}
