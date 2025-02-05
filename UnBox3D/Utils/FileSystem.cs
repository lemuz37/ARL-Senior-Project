using System.IO;

namespace UnBox3D.Utils
{
    public interface IFileSystem
    {
        bool DoesFileExists(string path);
        string ReadFile(string path);
        bool DoesDirectoryExists(string path);
        long GetFileSize(string path);
        void CreateDirectory(string path);
        void WriteToFile(string path, string content, bool append = true);
        void MoveFile(string sourcePath, string destinationPath);
        void DeleteFile(string path);
    }

    public class FileSystem : IFileSystem
    {
        public bool DoesFileExists(string path) => File.Exists(path);
        public string ReadFile(string path) => File.ReadAllText(path);
        public bool DoesDirectoryExists(string path) => Directory.Exists(path);
        public long GetFileSize(string path) => new FileInfo(path).Length;
        public void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        public void WriteToFile(string path, string content, bool append = true)
        {
            using (var writer = new StreamWriter(path, append))
            {
                writer.WriteLine(content);
            }
        }
        public void MoveFile(string sourcePath, string destinationPath) => File.Move(sourcePath, destinationPath);
        public void DeleteFile(string path) => File.Delete(path);
    }
}
