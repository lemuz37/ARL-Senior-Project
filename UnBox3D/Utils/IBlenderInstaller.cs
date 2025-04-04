using System.IO;
using System.Net.Http;
using System.IO.Compression;
using System.Diagnostics;

namespace UnBox3D.Utils
{
    public interface IBlenderInstaller
    {
        Task CheckAndInstallBlender();
    }

    public class BlenderInstaller : IBlenderInstaller
    {
        #region Fields
        private static readonly string BlenderFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Blender");
        private static readonly string BlenderExecutable = Path.Combine(BlenderFolder, "blender-4.2.0-windows-x64", "blender.exe");
        private static readonly string BlenderZipPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "blender.zip");
        private static readonly string BlenderDownloadUrl = "https://download.blender.org/release/Blender4.2/blender-4.2.0-windows-x64.zip";
        private Task? _blenderInstallTask;
        #endregion

        #region Public Methods
        public Task CheckAndInstallBlender()
        {
            if (_blenderInstallTask == null)
            {
                _blenderInstallTask = CheckAndInstallBlenderInternal();
            }
            return _blenderInstallTask;
        }
        #endregion

        #region Private Methods
        private async Task CheckAndInstallBlenderInternal()
        {
            if (!Directory.Exists(BlenderFolder) || !File.Exists(BlenderExecutable))
            {
                Debug.WriteLine("Blender 4.2 is not installed. Downloading now...");
                await DownloadAndExtractBlender();
            }
            else
            {
                Debug.WriteLine("Blender 4.2 is already installed.");
            }
        }

        private static async Task DownloadAndExtractBlender()
        {
            using HttpClient client = new HttpClient();
            var response = await client.GetAsync(BlenderDownloadUrl);
            response.EnsureSuccessStatusCode();

            byte[] data = await response.Content.ReadAsByteArrayAsync();
            await File.WriteAllBytesAsync(BlenderZipPath, data);

            Debug.WriteLine("Download complete. Extracting Blender...");

            if (!Directory.Exists(BlenderFolder))
            {
                Directory.CreateDirectory(BlenderFolder);
            }

            ZipFile.ExtractToDirectory(BlenderZipPath, BlenderFolder, overwriteFiles: true);
            File.Delete(BlenderZipPath);

            Debug.WriteLine("Blender installation completed.");
        }
        #endregion
    }
}
