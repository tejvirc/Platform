namespace Aristocrat.Monaco.Common.Storage
{
    using System.IO;

    /// <summary>
    ///     Wrapper to work with stream or file system.
    /// </summary>
    public interface IFileSystemProvider
    {
        /// <summary>
        ///     Gets write stream instance by file path.
        /// </summary>
        /// <param name="filePath">File path.</param>
        /// <returns>Returns stream instance.</returns>
        Stream GetFileWriteStream(string filePath);

        /// <summary>
        ///     Gets read stream instance by file path.
        /// </summary>
        /// <param name="filePath">File path.</param>
        /// <returns>Returns stream instance.</returns>
        Stream GetFileReadStream(string filePath);

        /// <summary>
        ///     Search files in specified directory by specified search pattern.
        /// </summary>
        /// <param name="directoryPath">Search directory full path.</param>
        /// <param name="pattern">Search pattern.</param>
        /// <returns>an array of files</returns>
        string[] SearchFiles(string directoryPath, string pattern);

        /// <summary>
        ///     Deletes specified directory.
        /// </summary>
        /// <param name="directoryPath">Directory path.</param>
        void DeleteFolder(string directoryPath);

        /// <summary>
        ///     Deletes specified file.
        /// </summary>
        /// <param name="filePath">File path.</param>
        void DeleteFile(string filePath);

        /// <summary>
        ///     Creates specified directory.
        /// </summary>
        /// <param name="directoryPath">Directory path.</param>
        /// <returns>the directory information</returns>
        DirectoryInfo CreateFolder(string directoryPath);

        /// <summary>
        ///     Creates specified file.
        /// </summary>
        /// <param name="filePath">file path.</param>
        /// <returns>the directory information</returns>
        FileInfo CreateFile(string filePath);
    }
}