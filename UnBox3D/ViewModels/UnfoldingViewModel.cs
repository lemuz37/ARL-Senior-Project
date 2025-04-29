using System.Diagnostics;
using System.IO;
using PdfSharpCore.Pdf;
using UnBox3D.Utils;
using UnBox3D.Views;

namespace UnBox3D.ViewModels
{
    public class UnfoldingViewModel
    {
        private readonly ISettingsManager _settingsManager;
        private readonly ILogger _logger;
        private readonly IFileSystem _fileSystem;
        private readonly IBlenderInstaller _blenderInstaller;
        private readonly BlenderIntegration _blenderIntegration;
        private readonly SVGEditor _svgEditor;

        public UnfoldingViewModel(
            ISettingsManager settingsManager,
            ILogger logger,
            IFileSystem fileSystem,
            IBlenderInstaller blenderInstaller,
            BlenderIntegration blenderIntegration,
            SVGEditor svgEditor)
        {
            _settingsManager = settingsManager;
            _logger = logger;
            _fileSystem = fileSystem;
            _blenderInstaller = blenderInstaller;
            _blenderIntegration = blenderIntegration;
            _svgEditor = svgEditor;
        }

        public async Task<bool> ProcessUnfoldingAsync(string inputModelPath, float pageWidth, float pageHeight)
        {
            Debug.WriteLine("Input model is coming from: " + inputModelPath);

            var installWindow = new LoadingWindow
            {
                StatusHint = "Installing Blender...",
                Owner = System.Windows.Application.Current.MainWindow,
                IsProgressIndeterminate = false
            };
            installWindow.Show();

            var installProgress = new Progress<double>(value =>
            {
                installWindow.UpdateProgress(value * 100);
                installWindow.UpdateStatus($"Installing Blender... {Math.Round(value * 100)}%");
            });

            await _blenderInstaller.CheckAndInstallBlender(installProgress);
            installWindow.Close();

            if (!_blenderInstaller.IsBlenderInstalled())
            {
                throw new InvalidOperationException("Blender is required to unfold models but was not found.");
            }

            var loadingWindow = new LoadingWindow
            {
                StatusHint = "This may take several minutes depending on model complexity",
                Owner = System.Windows.Application.Current.MainWindow,
                IsProgressIndeterminate = false
            };
            loadingWindow.Show();

            try
            {
                if (pageWidth <= 0 || pageHeight <= 0)
                    throw new InvalidOperationException("Page Dimensions cannot be 0.");

                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string outputDirectory = _fileSystem.CombinePaths(baseDir, "UnfoldedOutputs");
                string scriptPath = _fileSystem.CombinePaths(baseDir, "Scripts", "unfolding_script.py");

                if (!_fileSystem.DoesDirectoryExist(outputDirectory))
                {
                    _fileSystem.CreateDirectory(outputDirectory);
                }

                CleanupUnfoldedFolder(outputDirectory);

                double incrementWidth = pageWidth;
                double incrementHeight = pageHeight;
                bool success = false;
                string errorMessage = "";
                int iteration = 0;

                loadingWindow.UpdateStatus("Preparing Blender environment...");
                await DispatcherHelper.DoEvents();

                while (!success)
                {
                    iteration++;
                    loadingWindow.UpdateStatus($"Processing with Blender (Attempt {iteration})...");
                    loadingWindow.UpdateProgress((double)iteration / 100 * 50);
                    await DispatcherHelper.DoEvents();

                    success = await Task.Run(() => _blenderIntegration.RunBlenderScript(
                        inputModelPath, outputDirectory, scriptPath,
                        "UnfoldTemp", incrementWidth, incrementHeight, "SVG", out errorMessage));

                    if (!success)
                    {
                        if (errorMessage.Contains("continue"))
                        {
                            incrementWidth++;
                            incrementHeight++;
                            loadingWindow.UpdateStatus($"Retrying with new dimensions: {incrementWidth} x {incrementHeight}");
                            await DispatcherHelper.DoEvents();
                        }
                        else
                        {
                            throw new TimeoutException("Unfolding took too long or failed with a non-recoverable error.");
                        }
                    }
                }

                loadingWindow.UpdateStatus("Processing unfolded panels...");
                await DispatcherHelper.DoEvents();

                string[] svgPanelFiles = Directory.GetFiles(outputDirectory, "*.svg");
                int totalPanels = svgPanelFiles.Length;

                for (int i = 0; i < totalPanels; i++)
                {
                    string svgFile = svgPanelFiles[i];
                    loadingWindow.UpdateStatus($"Processing panel {i + 1} of {totalPanels}");
                    loadingWindow.UpdateProgress(50 + ((double)i / totalPanels * 30));
                    await DispatcherHelper.DoEvents();

                    await Task.Run(() => _svgEditor.ExportSvgPanels(svgFile, outputDirectory, "UnfoldTemp", i,
                        pageWidth * 1000f, pageHeight * 1000f));
                }

                loadingWindow.UpdateStatus("Waiting for export location...");
                await DispatcherHelper.DoEvents();

                Microsoft.Win32.SaveFileDialog saveFileDialog = new()
                {
                    Title = "Save your unfolded file",
                    Filter = "SVG Files|*.svg|PDF Files|*.pdf",
                    FileName = "MyUnfoldedFile"
                };

                if (saveFileDialog.ShowDialog() != true)
                {
                    return false;
                }

                string filePath = saveFileDialog.FileName;
                string ext = Path.GetExtension(filePath).ToLowerInvariant();
                string? userSelectedPath = Path.GetDirectoryName(filePath);

                if (string.IsNullOrEmpty(userSelectedPath))
                    throw new InvalidOperationException("Unable to determine the selected directory.");

                string newFileName = Path.GetFileNameWithoutExtension(filePath);
                string format = ext == ".pdf" ? "PDF" : "SVG";

                foreach (string file in Directory.GetFiles(outputDirectory, "UnfoldTemp*"))
                {
                    string newPath = Path.Combine(outputDirectory,
                        Path.GetFileName(file).Replace("UnfoldTemp", newFileName));
                    File.Move(file, newPath, true);
                }

                loadingWindow.UpdateStatus("Exporting final files...");
                loadingWindow.UpdateProgress(80);
                await DispatcherHelper.DoEvents();

                if (format == "SVG")
                {
                    string[] svgFiles = Directory.GetFiles(outputDirectory, $"{newFileName}*.svg");
                    foreach (var file in svgFiles)
                    {
                        string destination = Path.Combine(userSelectedPath, Path.GetFileName(file));
                        File.Move(file, destination, true);
                    }
                }
                else if (format == "PDF")
                {
                    string pdfFile = Path.Combine(outputDirectory, $"{newFileName}.pdf");
                    string[] svgFiles = Directory.GetFiles(outputDirectory, $"{newFileName}_panel_page*.svg");
                    var pdf = new PdfDocument();

                    foreach (var svgFile in svgFiles)
                    {
                        bool ok = await Task.Run(() => _svgEditor.ExportToPdf(svgFile, pdf));
                        if (!ok)
                        {
                            return false;
                        }
                    }

                    pdf.Save(Path.Combine(userSelectedPath, $"{newFileName}.pdf"));
                }

                loadingWindow.UpdateProgress(100);
                CleanupUnfoldedFolder(outputDirectory);
                return true;
            }
            finally
            {
                loadingWindow.Close();
            }
        }

        private void CleanupUnfoldedFolder(string folderPath)
        {
            try
            {
                string[] files = Directory.GetFiles(folderPath);
                foreach (string file in files)
                {
                    File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                _logger.Warn("Failed to cleanup temporary unfolding files: " + ex.Message);
            }
        }
    }
}