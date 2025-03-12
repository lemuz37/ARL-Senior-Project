using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Text.RegularExpressions;

namespace UnBox3D.Views
{
    /// <summary>
    /// Interaction logic for StructureSupportWindow.xaml
    /// </summary>
    public partial class StructureSupportWindow : Window
    {
        public StructureSupportWindow()
        {
            InitializeComponent();
            DataContext = new ViewModels.StructureSupportViewModel();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"[^0-9\.]");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
