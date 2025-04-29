using System.Diagnostics;
using System.IO;
using UnBox3D.Models;
using UnBox3D.Rendering;
using UnBox3D.Utils;
using UnBox3D.Views;

namespace UnBox3D.ViewModels
{
    public class SimplificationViewModel
    {
        private readonly ISettingsManager _settingsManager;
        private readonly ILogger _logger;
        private readonly IFileSystem _fileSystem;
        private readonly ISceneManager _sceneManager;
        private readonly ModelImporter _modelImporter;
        private readonly ModelExporter _modelExporter;

        public SimplificationViewModel(ISettingsManager settingsManager, ILogger logger, IFileSystem fileSystem,
                                        ISceneManager sceneManager, ModelExporter modelExporter)
        {
            _settingsManager = settingsManager;
            _logger = logger;
            _fileSystem = fileSystem;
            _sceneManager = sceneManager;
            _modelExporter = modelExporter;
            _modelImporter = new ModelImporter(_settingsManager);
        }

        public async Task<bool> SimplifyAllAsync(string method, float ratio)
        {
            var meshes = _sceneManager.GetMeshes().ToList();
            if (meshes.Count == 0)
                return false;

            var loadingWindow = new LoadingWindow
            {
                StatusHint = $"Simplifying all meshes… This may take a few moments.",
                Owner = System.Windows.Application.Current.MainWindow,
                IsProgressIndeterminate = true
            };
            loadingWindow.Show();

            UpdateLoadingStatus(loadingWindow, method);

            try
            {
                string? exportFile = _modelExporter.ExportToObj(meshes, $"scene_to_simplify.obj");
                if (exportFile == null)
                    return false;

                string simplifiedOutput = Path.Combine(Path.GetTempPath(), $"simplified_scene_{method}.obj");
                string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts", "simplify.exe");
                float scaledRatio = Math.Clamp(ratio, 10, 100) / 100f;

                var startInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = $"\"{exportFile}\" \"{simplifiedOutput}\" \"{method}\" \"{scaledRatio}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = startInfo };
                process.Start();
                await process.StandardOutput.ReadToEndAsync();
                await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                    return false;

                loadingWindow.UpdateStatus("Importing simplified scene...");

                var simplifiedMeshes = _modelImporter.ImportModel(simplifiedOutput);
                if (simplifiedMeshes.Count == 0)
                    return false;

                _sceneManager.ClearScene();
                foreach (var mesh in simplifiedMeshes)
                    _sceneManager.AddMesh(mesh);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"SimplifyAllAsync exception: {ex.Message}");
                return false;
            }
            finally
            {
                loadingWindow.Close();
            }
        }

        public async Task<bool> SimplifyMeshAsync(IAppMesh mesh, string method, float ratio)
        {
            if (mesh == null)
                return false;

            var loadingWindow = new LoadingWindow
            {
                StatusHint = $"Simplifying mesh…",
                Owner = System.Windows.Application.Current.MainWindow,
                IsProgressIndeterminate = true
            };
            loadingWindow.Show();

            UpdateLoadingStatus(loadingWindow, method);

            try
            {
                string tempName = $"temp_singlemesh_{Guid.NewGuid()}.obj";
                string? tempFile = _modelExporter.ExportToObj(new[] { mesh }.ToList(), tempName);
                if (tempFile == null)
                    return false;

                string baseOutput = Path.Combine(Path.GetTempPath(), $"simplified_{method}.obj");
                string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts", "simplify.exe");
                float scaledRatio = Math.Clamp(ratio, 10, 100) / 100f;

                var startInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = $"\"{tempFile}\" \"{baseOutput}\" \"{method}\" \"{scaledRatio}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = startInfo };
                process.Start();
                await process.StandardOutput.ReadToEndAsync();
                await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                    return false;

                loadingWindow.UpdateStatus("Importing simplified mesh...");

                var simplifiedMeshes = _modelImporter.ImportModel(baseOutput);
                if (simplifiedMeshes.Count == 0)
                    return false;

                _sceneManager.ReplaceMesh(mesh, simplifiedMeshes[0]);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"SimplifyMeshAsync exception: {ex.Message}");
                return false;
            }
            finally
            {
                loadingWindow.Close();
            }
        }

        private static void UpdateLoadingStatus(LoadingWindow loadingWindow, string method)
        {
            if (method == "quadric_edge_collapse")
                loadingWindow.UpdateStatus("Quadric Edge Collapse Simplification");
            else if (method == "fast_quadric_decimation")
                loadingWindow.UpdateStatus("Fast Quadric Decimation");
            else if (method == "vertex_clustering")
                loadingWindow.UpdateStatus("Vertex Clustering Simplification");
        }
    }
}
