using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using System.IO;

namespace UnBox3D.ViewModels
{
    public partial class StructureSupportViewModel : ObservableObject
    {
        // Automatically generates properties with OnPropertyChanged
        [ObservableProperty]
        private double shortEdgeValue;

        [ObservableProperty]
        private double mediumEdgeValue;

        [ObservableProperty]
        private double longEdgeValue;

        // Command to handle Apply button
        [RelayCommand]
        private void ApplyDimensions()
        {
            // Apply the calculations for each edge with rounding
            double resultShort = Math.Round(shortEdgeValue * 0.8846, 1);
            double resultMedium = Math.Round(mediumEdgeValue * 0.9107, 1);
            double resultLong = Math.Round(longEdgeValue * 0.8809, 1);

            // Calculate the Equilateral Beam after applying dimensions
            double equilateralBeamValue = Math.Round(((resultShort + resultMedium + resultLong) / 3) * 0.87, 2);

            // Show the result in the ViewModel and bind to the view
            System.Windows.MessageBox.Show($"Results (multiplied by their respective factors):\n" +
                    $"Short Edge: {resultShort} in\n" +
                    $"Medium Edge: {resultMedium} in\n" +
                    $"Long Edge: {resultLong} in\n" +
                    $"Equilateral Beam: {equilateralBeamValue} in ",
                    "Calculated Dimensions", MessageBoxButton.OK, MessageBoxImage.Information);

            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "beams.obj");

            try
            {
                BeamArrayExporter.GenerateRightAngleBeamsObjFile(
                    filePath,
                    (float)resultShort,
                    (float)resultMedium,
                    (float)resultLong
                );

                System.Windows.MessageBox.Show($"OBJ file successfully exported to:\n{filePath}", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to export OBJ file:\n{ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }


        }
    }
}

