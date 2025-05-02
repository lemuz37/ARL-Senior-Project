using System.Windows;
using UnBox3D.ViewModels;

namespace UnBox3D.Views
{
    public partial class SimplifyWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public SimplifyWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            Owner = System.Windows.Application.Current.MainWindow;
        }

        private void RemoveSmallMeshes_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SmallMeshThreshold = (float)ThresholdSlider.Value;
            _viewModel.RemoveSmallMeshesCommand.Execute(null);
            Close();
        }

        private void ReplaceWithBoundingBoxes_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ReplaceSceneWithBoundingBoxesCommand.Execute(null);
            Close();
        }

        private void Simplify_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SimplificationRatio = (float)RatioSlider.Value;

            int index = QualityComboBox.SelectedIndex;
            switch (index)
            {
                case 0:
                    _viewModel.SimplifyAllQECCommand.Execute(null);
                    break;
                case 1:
                    _viewModel.SimplifyAllFQDCommand.Execute(null);
                    break;
                case 2:
                    _viewModel.SimplifyAllVCCommand.Execute(null);
                    break;
            }

            Close();
        }
    }

}