using Moq;
using System.IO;
using UnBox3D.Utils;
using Xunit;

namespace UnBox3D.Tests
{
    public class SettingsManagerTests
    {
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<ILogger> _loggerMock;

        public SettingsManagerTests()
        {
            _fileSystemMock = new Mock<IFileSystem>();
            _loggerMock = new Mock<ILogger>();
        }

        [Fact]
        public void SettingsManager_ShouldInitializeWithDefaultSettings_WhenSettingsFileDoesNotExist()
        {
            // Arrange
            _fileSystemMock.Setup(fs => fs.DoesFileExists(It.IsAny<string>())).Returns(false);

            // Act
            var settingsManager = new SettingsManager(_fileSystemMock.Object, _loggerMock.Object);

            // Assert
            Assert.NotNull(settingsManager);
            _loggerMock.Verify(_loggerMock => _loggerMock.Info(It.Is<string>(s => s.Contains("Settings directory not found. Creating directory: C:\\ProgramData\\UnBox3D\\Settings"))), Times.Once);
        }



        [Fact]
        public void SettingsManager_ShouldLoadExistingSettings_WhenSettingsFileExists()
        {
            // Arrange
            string existingSettings = @"{ ""AppSettings"": { ""SplashScreenDuration"" : 5.0 } }";
            _fileSystemMock.Setup(fs => fs.DoesFileExists(It.IsAny<string>())).Returns(true);
            _fileSystemMock.Setup(fs => fs.ReadFile(It.IsAny<string>())).Returns(existingSettings);

            // Act
            var settingsManager = new SettingsManager(_fileSystemMock.Object, _loggerMock.Object);

            // Assert
            var splashScreenDuration = settingsManager.GetSetting(3.0f, "AppSettings", "SplashScreenDuration");
            Assert.Equal(5.0f, splashScreenDuration);
            _loggerMock.Verify(logger => logger.Info(It.Is<string>(s => s.Contains("Settings loaded successfully"))), Times.Once);
        }

    [Fact]
        public void SettingsManager_ShouldSaveSettings_WhenSaveSettingsIsCalled()
        {
            // Arrange
            _fileSystemMock.Setup(fs => fs.WriteToFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()));

            // Act
            var settingsManager = new SettingsManager(_fileSystemMock.Object, _loggerMock.Object);

            // Assert
            _fileSystemMock.Verify(fs => fs.WriteToFile(It.IsAny<string>(), It.IsAny<string>(), true), Times.Once);
        }

        [Fact]
        public void SettingsManager_ShouldReturnCorrectValue_WhenGettingExistingSetting()
        {
            // Arrange
            string existingSettings = @"{ ""AppSettings"": { ""SplashScreenDuration"" : 5.0 } }";
            _fileSystemMock.Setup(fs => fs.DoesFileExists(It.IsAny<string>())).Returns(true);
            _fileSystemMock.Setup(fs => fs.ReadFile(It.IsAny<string>())).Returns(existingSettings);

            var settingsManager = new SettingsManager(_fileSystemMock.Object, _loggerMock.Object);

            // Act
            var splashScreenDuration = settingsManager.GetSetting(3.0f, "AppSettings", "SplashScreenDuration");

            // Assert
            Assert.Equal(5.0f, splashScreenDuration);
        }

        [Fact]
        public void SettingsManager_ShouldUpdateSetting_WhenUpdateSettingIsCalled()
        {
            // Arrange
            var settingsManager = new SettingsManager(_fileSystemMock.Object, _loggerMock.Object);

            // Act
            settingsManager.UpdateSetting(10.0f, "AppSettings", "SplashScreenDuration");

            // Assert
            var updatedValue = settingsManager.GetSetting(3.0f, "AppSettings", "SplashScreenDuration");
             Assert.Equal(10.0f, updatedValue);
            _loggerMock.Verify(logger => logger.Info(It.Is<string>(s => s.Contains("Updated AppSettings -> SplashScreenDuration"))), Times.Once);
        }

        [Fact]
        public void SettingsManager_ShouldUpdateThreeLevelNestedSetting_WhenUpdateSettingIsCalled()
        {
            // Arrange
            var settingsManager = new SettingsManager(_fileSystemMock.Object, _loggerMock.Object);

            // Act
            settingsManager.UpdateSetting(false, "AssimpSettings", "Import", "EnableTriangulation");

            // Assert
            var updatedValue = settingsManager.GetSetting(true, "AssimpSettings", "Import", "EnableTriangulation");
            Assert.False(updatedValue);
            _loggerMock.Verify(logger => logger.Info(It.Is<string>(s => s.Contains("Updated AssimpSettings -> Import -> EnableTriangulation"))), Times.Once);
        }
    }
}