using System.IO;
using System.Net.Http;
using System.IO.Compression;
using System.Windows;
using WpfMessageBox = System.Windows.MessageBox;


namespace UnBox3D.Utils
{
    public interface IBlenderInstaller
    {
        Task CheckAndInstallBlender();
        Task CheckAndInstallBlender(IProgress<double> progress);
        bool IsBlenderInstalled();
    }

    public class BlenderInstaller : IBlenderInstaller
    {
        private readonly IFileSystem _fileSystem;
        private readonly ILogger _logger;

        private readonly string BlenderFolder;
        private readonly string BlenderExecutable;
        private readonly string BlenderZipPath;
        private static readonly string BlenderDownloadUrl = "https://download.blender.org/release/Blender4.2/blender-4.2.0-windows-x64.zip";
        private Task? _blenderInstallTask;

        public BlenderInstaller(IFileSystem fileSystem, ILogger logger)
        {
            _fileSystem = fileSystem;
            _logger = logger;

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            BlenderFolder = _fileSystem.CombinePaths(baseDir, "Blender");
            BlenderExecutable = _fileSystem.CombinePaths(BlenderFolder, "blender-4.2.0-windows-x64", "blender.exe");
            BlenderZipPath = _fileSystem.CombinePaths(baseDir, "blender.zip");
        }

        public Task CheckAndInstallBlender()
        {
            return CheckAndInstallBlenderInternal();
        }

        public Task CheckAndInstallBlender(IProgress<double> progress)
        {
            return CheckAndInstallBlenderInternal(progress);
        }

        public bool IsBlenderInstalled()
        {
            return _fileSystem.DoesFileExist(BlenderExecutable);
        }

        private async Task CheckAndInstallBlenderInternal()
        {
            if (!_fileSystem.DoesDirectoryExist(BlenderFolder) || !_fileSystem.DoesFileExist(BlenderExecutable))
            {
                _logger.Info("Blender 4.2 is not installed. Downloading now...");
                await DownloadAndExtractBlender();
            }
            else
            {
                _logger.Info("Blender 4.2 is already installed.");
            }
        }

        private async Task CheckAndInstallBlenderInternal(IProgress<double>? progress = null)
        {
            if (!_fileSystem.DoesDirectoryExist(BlenderFolder) || !_fileSystem.DoesFileExist(BlenderExecutable))
            {
                _logger.Info("Blender 4.2 is not installed. Downloading now...");

                try
                {
                    await DownloadAndExtractBlender(progress);
                }
                catch (HttpRequestException ex)
                {
                    _logger.Error("Internet connection error: " + ex.Message);
                    WpfMessageBox.Show("Blender could not be downloaded. Please check your internet connection.",
                                    "Download Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                _logger.Info("Blender 4.2 is already installed.");
                progress?.Report(1.0);
            }
        }

        private async Task DownloadAndExtractBlender()
        {
            using HttpClient client = new HttpClient();
            var response = await client.GetAsync(BlenderDownloadUrl);
            response.EnsureSuccessStatusCode();

            byte[] data = await response.Content.ReadAsByteArrayAsync();
            await _fileSystem.WriteAllBytesAsync(BlenderZipPath, data);

            _logger.Info("Download complete. Extracting Blender...");

            if (!_fileSystem.DoesDirectoryExist(BlenderFolder))
                _fileSystem.CreateDirectory(BlenderFolder);

            try
            {
                ZipFile.ExtractToDirectory(BlenderZipPath, BlenderFolder, overwriteFiles: true);
                _fileSystem.DeleteFile(BlenderZipPath);
                _logger.Info("Blender installation completed.");
            }
            catch (IOException ioEx)
            {
                _logger.Error($"IO error during extraction: {ioEx.Message}");
                WpfMessageBox.Show("Blender archive is currently in use or locked by another process. Please close any applications using it and try again.",
                                "Extraction Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (InvalidDataException dataEx)
            {
                _logger.Error($"Extraction failed: Invalid or corrupted zip file. {dataEx.Message}");
                WpfMessageBox.Show("The downloaded Blender archive appears to be corrupted. Please try downloading again.",
                                "Corrupt Archive", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (UnauthorizedAccessException accessEx)
            {
                _logger.Error($"Permission issue during extraction: {accessEx.Message}");
                WpfMessageBox.Show("Access denied while extracting Blender files. Please run the application as administrator.",
                                "Permission Denied", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error during extraction: {ex.Message}");
                WpfMessageBox.Show("An unexpected error occurred while extracting Blender. Please try again or contact support.",
                                "Extraction Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DownloadAndExtractBlender(IProgress<double>? progress = null)
        {
            using HttpClient client = new HttpClient();
            var response = await client.GetAsync(BlenderDownloadUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            var downloadedBytes = 0L;
            var buffer = new byte[8192];

            await using (var contentStream = await response.Content.ReadAsStreamAsync())
            await using (var fileStream = _fileSystem.CreateFile(BlenderZipPath))
            {
                int bytesRead;
                while ((bytesRead = await contentStream.ReadAsync(buffer)) > 0)
                {
                    await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                    downloadedBytes += bytesRead;
                    if (totalBytes > 0)
                        progress?.Report((double)downloadedBytes / totalBytes * 0.8);
                }
            }

            _logger.Info("Download complete. Extracting Blender...");

            if (!_fileSystem.DoesDirectoryExist(BlenderFolder))
                _fileSystem.CreateDirectory(BlenderFolder);

            progress?.Report(0.85);
            try
            {
                ZipFile.ExtractToDirectory(BlenderZipPath, BlenderFolder, overwriteFiles: true);
                _fileSystem.DeleteFile(BlenderZipPath);
                progress?.Report(1.0);
                _logger.Info("Blender installation completed.");
            }
            catch (IOException ioEx)
            {
                _logger.Error($"IO error during extraction: {ioEx.Message}");
                WpfMessageBox.Show("Blender archive is currently in use or locked by another process. Please close any applications using it and try again.",
                                "Extraction Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (InvalidDataException dataEx)
            {
                _logger.Error($"Extraction failed: Invalid or corrupted zip file. {dataEx.Message}");
                WpfMessageBox.Show("The downloaded Blender archive appears to be corrupted. Please try downloading again.",
                                "Corrupt Archive", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (UnauthorizedAccessException accessEx)
            {
                _logger.Error($"Permission issue during extraction: {accessEx.Message}");
                WpfMessageBox.Show("Access denied while extracting Blender files. Please run the application as administrator.",
                                "Permission Denied", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error during extraction: {ex.Message}");
                WpfMessageBox.Show("An unexpected error occurred while extracting Blender. Please try again or contact support.",
                                "Extraction Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
