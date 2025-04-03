using System.Windows;


namespace UnBox3D.Views
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = App.Current.Windows
                .OfType<MainWindow>()
                .FirstOrDefault();

            mainWindow?.Show();
            this.Close();
        }
    }
}

