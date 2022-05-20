namespace Aristocrat.Monaco.Common.Storage
{
    using System.IO;
    using System.Linq;

    /// <summary>
    ///     Wrapper to work with stream or file system.
    /// </summary>
    public class FileSystemProvider : IFileSystemProvider
    {
        /// <inheritdoc />
        public Stream GetFileWriteStream(string filePath)
        {
            return new FileStream(filePath, FileMode.Create);
        }

        /// <inheritdoc />
        public Stream GetFileReadStream(string filePath)
        {
            return new FileStream(filePath, FileMode.Open);
        }

        /// <inheritdoc />
        public string[] SearchFiles(string directoryPath, string pattern)
        {
            return new DirectoryInfo(directoryPath).GetFiles(pattern).Select(file => file.FullName).ToArray();
        }

        /// <inheritdoc />
        public void DeleteFolder(string directoryPath)
        {
            var directoryInfo = new DirectoryInfo(directoryPath);

            if (directoryInfo.Exists)
            {
                directoryInfo.Delete(true);
            }
        }

        /// <inheritdoc />
        public void DeleteFile(string filePath)
        {
            var fileInfo = new FileInfo(filePath);

            if (fileInfo.Exists)
            {
                fileInfo.Delete();
            }
        }

        /// <inheritdoc />
        public DirectoryInfo CreateFolder(string directoryPath)
        {
            var directoryInfo = new DirectoryInfo(directoryPath);

            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }

            return directoryInfo;
        }

        /// <inheritdoc />
        public FileInfo CreateFile(string filePath)
        {
            var fileInfo = new FileInfo(filePath);

            if (fileInfo.Exists)
            {
                return fileInfo;
            }

            using (var _ = fileInfo.Create())
            {
                return fileInfo;
            }
        }
    }
}