using System.Windows;
using System.Windows.Controls;
using UnBox3D.Rendering;
using UnBox3D.ViewModels;

namespace UnBox3D.Views
{
    public partial class ScaleModelWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public ScaleModelWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            Owner = System.Windows.Application.Current.MainWindow;
        }

        private void ApplyScale_Click(object sender, RoutedEventArgs e)
        {
            if (float.TryParse(TargetDimensionBox.Text, out float targetSize))
            {
                Axis selectedAxis = Axis.X; // Default

                if (AxisComboBox.SelectedItem is ComboBoxItem selectedItem)
                {
                    string axis = selectedItem.Content.ToString();
                    selectedAxis = axis switch
                    {
                        "Width" => Axis.X,
                        "Height" => Axis.Y,
                        "Depth" => Axis.Z,
                        _ => Axis.X
                    };
                }

                _viewModel.ResizeAllMeshesCommand.Execute((targetSize, selectedAxis));
                this.Close();
            }
            else
            {
                System.Windows.MessageBox.Show("Please enter a valid number.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void NumericTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, 0) && e.Text != ".";
        }
    }
}
