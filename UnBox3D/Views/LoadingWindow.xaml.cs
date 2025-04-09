using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Application = System.Windows.Application;

namespace UnBox3D.Views
{
    public partial class LoadingWindow : Window, INotifyPropertyChanged
    {
        private string _statusHint;

        public LoadingWindow()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            DataContext = this; // ensure bindings to properties work
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

        public string StatusHint
        {
            get => _statusHint;
            set
            {
                if (_statusHint != value)
                {
                    _statusHint = value;
                    OnPropertyChanged(nameof(StatusHint));
                }
            }
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

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
