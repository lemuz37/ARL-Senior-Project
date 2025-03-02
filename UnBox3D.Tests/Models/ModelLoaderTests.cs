using System.IO;
using System.Linq;
using Xunit;
using FluentAssertions;
using UnBox3D.Rendering;
using UnBox3D.Models;

namespace UnBox3D.Tests.Models
{
    public class ModelLoaderTests
    {
        [Fact]
        public void LoadModel_ValidFile_ShouldLoadAppMeshes()
        {
            // Arrange
            //var modelLoader = new ModelImporter();
            var testFilePath = Path.Combine(Directory.GetCurrentDirectory(), "test_model.obj");

            // Act
            //var appMeshes = ModelImporter.ImportModel(testFilePath);

            // Assert
            //appMeshes.Should().NotBeNull();                  // Ensure the returned list is not null
            //appMeshes.Should().NotBeEmpty();                // Ensure at least one mesh is loaded
            //appMeshes.First().GetAssimpMesh().VertexCount.Should().BeGreaterThan(0); // Ensure the first mesh has vertices
            //appMeshes.First().GetG3Mesh().TriangleCount.Should().BeGreaterThan(0); // Ensure the first mesh has triangles
        }
    }
}
