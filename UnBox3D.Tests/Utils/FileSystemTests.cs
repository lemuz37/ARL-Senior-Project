using System.IO;
using Moq;
using UnBox3D.Utils;
using Xunit;

namespace UnBox3D.Tests.Utils
{
    // TODO: Add more granular test cases for FileSystem operations that mutate filesystem state.
    public class FileSystemTests
    {
        static readonly string sampleDirectoryPath = Path.GetFullPath("../../../"); // NOTE: Contains root of testing code outside of bin/(Debug | Release)
        static readonly string sampleSubdirectoryPath = Path.Combine(sampleDirectoryPath, "Models");
        static readonly string sampleDudSubdirectoryPath = Path.Combine(sampleDirectoryPath, "Extra");
        static readonly string sampleRealFilePath = Path.Combine(sampleSubdirectoryPath, "CommandHistoryTests.cs");
        static readonly string sampleNoFilePath = Path.Combine(sampleDudSubdirectoryPath, "Foo.cs");

        public FileSystemTests() { }

        [Fact]
        public void FileSystem_ShouldNotFindDirectory_WhenNotPresent()
        {
            var fileSystem = new FileSystem();

            var absentDirectoryResult = fileSystem.DoesDirectoryExists(sampleDudSubdirectoryPath);

            Assert.False(absentDirectoryResult);
        }

        [Fact]
        public void FileSystem_ShouldFindDirectory_WhenPresent()
        {
            var fileSystem = new FileSystem();

            var presentDirectoryResult = fileSystem.DoesDirectoryExists(sampleDirectoryPath);

            Assert.True(presentDirectoryResult);
        }

        [Fact]
        public void FileSystem_ShouldNotFindFile_WhenNotPresent()
        {
            var fileSystem = new FileSystem();

            var hasFileResult = fileSystem.DoesFileExists(sampleNoFilePath);

            Assert.False(hasFileResult);
        }

        [Fact]
        public void FileSystem_ShouldFindFile_WhenPresent()
        {
            var fileSystem = new FileSystem();

            var hasFileResult = fileSystem.DoesFileExists(sampleRealFilePath);

            Assert.True(hasFileResult);
        }

        [Fact]
        public void FileSystem_ShouldGiveDudFilePath_WhenJoinDudDirectoryAndFile()
        {
            var fileSystem = new FileSystem();
            string[] tempElements = { sampleDudSubdirectoryPath, "Foo.cs" };

            var tempFilePath = fileSystem.CombinePaths(tempElements);

            Assert.Equal(tempFilePath, sampleNoFilePath);
        }

        [Fact]
        public void FileSystem_ShouldGiveRealFilePath_WhenJoinRealDirectoryAndFile()
        {
            var fileSystem = new FileSystem();

            string[] tempElements = { sampleSubdirectoryPath, "CommandHistoryTests.cs" };

            var tempFilePath = fileSystem.CombinePaths(tempElements);

            Assert.Equal(tempFilePath, sampleRealFilePath);
        }
    }
}
