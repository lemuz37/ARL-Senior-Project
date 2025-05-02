using Moq;
using System;
using Xunit;
using UnBox3D.Utils;

namespace UnBox3D.Tests.Utils
{
    public class LoggerTests
    {
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Logger _logger;
        private readonly string _logDirectory = "C:\\Temp\\Logs";
        private readonly string _logFilePath;

        public LoggerTests()
        {
            _fileSystemMock = new Mock<IFileSystem>();
            _logFilePath = $"{_logDirectory}\\UnBox3D.log";
            _logger = new Logger(_fileSystemMock.Object, _logDirectory);
        }

        [Fact]
        public void Log_WritesMessageToFile()
        {
            // Arrange
            string logMessage = "Test message";
            _fileSystemMock.Setup(fs => fs.DoesFileExist(_logFilePath)).Returns(false);

            // Act
            _logger.Info(logMessage);

            // Assert
            _fileSystemMock.Verify(fs => fs.WriteToFile(_logFilePath, It.Is<string>(s => s.Contains(logMessage))), Times.Once);
        }

        [Fact]
        public void Log_RotatesLogs_WhenFileSizeExceedsLimit()
        {
            // Arrange
            _fileSystemMock.Setup(fs => fs.DoesFileExist(_logFilePath)).Returns(true);
            _fileSystemMock.Setup(fs => fs.GetFileSize(_logFilePath)).Returns(6 * 1024 * 1024); // 6MB

            // Act
            _logger.Info("This triggers log rotation");

            // Assert
            _fileSystemMock.Verify(fs => fs.MoveFile(_logFilePath, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void Log_CreatesDirectoryIfNotExists()
        {
            // Arrange
            _fileSystemMock.Setup(fs => fs.DoesFileExist(It.IsAny<string>())).Returns(false);
            _fileSystemMock.Setup(fs => fs.CreateDirectory(_logDirectory));

            // Act
            _logger.Info("Initialize logger");

            // Assert
            _fileSystemMock.Verify(fs => fs.CreateDirectory(_logDirectory), Times.Once);
        }
    }
}
