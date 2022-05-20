namespace Aristocrat.Monaco.Protocol.Common.Installer
{
    using System.IO;

    /// <summary>
    ///     Archiver to process Zip archive format.
    /// </summary>
    public interface IZipArchive
    {
        /// <summary>
        ///     Creates archive in target archive stream from source directory
        /// </summary>
        /// <param name="sourceDirectory">Path to directory to archivate.</param>
        /// <param name="targetArchiveStream">Target stream.</param>
        void Pack(string sourceDirectory, Stream targetArchiveStream);

        /// <summary>
        ///     Creates archive in target archive stream from source directory
        /// </summary>
        /// <param name="sourceDirectory">Path to directory to archivate.</param>
        /// <param name="fileName">File name.</param>
        /// <param name="targetArchiveStream">Target stream.</param>
        void Pack(string sourceDirectory, string fileName, Stream targetArchiveStream);

        /// <summary>
        ///     Extracts archive stream to target directory
        /// </summary>
        /// <param name="targetDirectory">Path to target directory where archive will be extracted.</param>
        /// <param name="archiveStream"><see cref="Stream" /> of archive.</param>
        void Unpack(string targetDirectory, Stream archiveStream);
    }
}