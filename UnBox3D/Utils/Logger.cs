using System;
using System.IO;

namespace UnBox3D.Utils
{
    public interface ILogger
    {
        void Info(string message);
        void Warn(string message);
        void Error(string message);
        void Fatal(string message);
        void LogException(Exception ex);
    }

    public class Logger : ILogger
    {
        #region Fields

        private readonly IFileSystem _fileSystem;
        private readonly object _lock = new object();
        private readonly string _logDirectory;
        private readonly string _logFilePath;
        private readonly long _maxFileSizeInBytes;
        private readonly string _logFileName;
        private readonly int _maxArchiveFiles;

        #endregion

        #region Constructors

        public Logger(
            IFileSystem fileSystem,
            string logDirectory = null,
            string logFileName = "UnBox3D.log",
            long maxFileSizeInBytes = 5 * 1024 * 1024,
            int maxArchiveFiles = 5)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _logDirectory = logDirectory ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "UnBox3D", "Log");
            _logFileName = logFileName;
            _maxFileSizeInBytes = maxFileSizeInBytes;
            _maxArchiveFiles = maxArchiveFiles;
            _logFilePath = Path.Combine(_logDirectory, _logFileName);

            if (!_fileSystem.DoesDirectoryExist(_logDirectory))
                _fileSystem.CreateDirectory(_logDirectory);

            Info($"Logger initialized. Writing to: {_logFilePath}");
        }

        #endregion

        #region Enums

        public enum LogLevel
        {
            Info,
            Debug,
            Warn,
            Error,
            Fatal
        }

        #endregion

        #region Public Logging Methods

        public void Log(string message, LogLevel level = LogLevel.Info)
        {
            try
            {
                lock (_lock)
                {
                    if (_fileSystem.DoesFileExist(_logFilePath) &&
                        _fileSystem.GetFileSize(_logFilePath) > _maxFileSizeInBytes)
                    {
                        Info("Rotating log files due to size limit.");
                        RotateLogs();
                    }

                    string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} " +
                                      $"[{level.ToString().ToUpper()}] {message}{Environment.NewLine}";

                    _fileSystem.AppendToFile(_logFilePath, logEntry);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Logging failed: {ex.Message}");
            }
        }

        public void Info(string message) => Log(message, LogLevel.Info);
        public void Warn(string message) => Log(message, LogLevel.Warn);
        public void Error(string message) => Log(message, LogLevel.Error);
        public void Fatal(string message) => Log(message, LogLevel.Fatal);

        public void LogException(Exception ex)
        {
            Error($"Exception: {ex.Message}\n{ex.StackTrace}");
        }

        #endregion

        #region Private Helper Methods

        private void RotateLogs()
        {
            for (int i = _maxArchiveFiles - 1; i >= 0; i--)
            {
                string oldLogFilePath = Path.Combine(_logDirectory, $"{_logFileName}.{i}");
                string newLogFilePath = Path.Combine(_logDirectory, $"{_logFileName}.{i + 1}");

                if (_fileSystem.DoesFileExist(newLogFilePath))
                    _fileSystem.DeleteFile(newLogFilePath);

                if (_fileSystem.DoesFileExist(oldLogFilePath))
                    _fileSystem.MoveFile(oldLogFilePath, newLogFilePath);
            }

            _fileSystem.MoveFile(_logFilePath, Path.Combine(_logDirectory, $"{_logFileName}.0"));
        }

        #endregion
    }
}
