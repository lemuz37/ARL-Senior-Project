using System.Windows;
using System.Windows.Controls;

namespace UnBox3D
{
    public partial class SettingsWindow : Window
    {
        public string SelectedBackgroundColor { get; private set; }
        public string SelectedMeshColor { get; private set; }

        public SettingsWindow()
        {
            InitializeComponent();
        }
    }
}
