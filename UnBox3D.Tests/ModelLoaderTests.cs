using Xunit;
using FluentAssertions;
using UnBox3D.Models;

namespace UnBox3D.Tests
{
    public class ModelLoaderTests
    {
        [Fact]
        public void LoadModel_ValidFile_ShouldLoadVertices()
        {
            // Arrange
            var modelLoader = new ModelLoader();
            var testFilePath = Path.Combine(Directory.GetCurrentDirectory(), "test_model.obj");

            // Act
            bool result = modelLoader.LoadModel(testFilePath);

            // Assert
            Assert.True(result);
            Assert.NotEmpty(modelLoader.Vertices);
        }
    }
}