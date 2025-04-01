using System;
using System.Windows;
using System.Windows.Controls;

namespace UnBox3D.Views
{
    public partial class LoadingWindow : Window
    {
        public LoadingWindow()
        {
            InitializeComponent();
            Owner = System.Windows.Application.Current.MainWindow;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        public void UpdateStatus(string message)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => UpdateStatus(message));
                return;
            }

            StatusText.Text = message;
        }

        public void UpdateProgress(double progressPercentage)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => UpdateProgress(progressPercentage));
                return;
            }

            progressPercentage = Math.Max(0, Math.Min(100, progressPercentage));
            ProgressBar.Value = progressPercentage;
        }
    }
}