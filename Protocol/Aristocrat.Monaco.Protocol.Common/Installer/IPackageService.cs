namespace Aristocrat.Monaco.Protocol.Common.Installer
{
    using System.IO;

    /// <summary>
    ///     Performs package pack/unpack operations.
    /// </summary>
    public interface IPackageService
    {
        /// <summary>
        ///     Creates archive in target archive stream">
        /// </summary>
        /// <param name="archiveFormat">
        ///     <see cref="ArchiveFormat" />
        /// </param>
        /// <param name="sourceDirectory">Path to package directory to archivate.</param>
        /// <param name="targetArchiveStream">Target stream to write archived data.</param>
        void Pack(ArchiveFormat archiveFormat, string sourceDirectory, Stream targetArchiveStream);

        /// <summary>
        ///     Creates archive in target archive stream">
        /// </summary>
        /// <param name="archiveFormat">
        ///     <see cref="ArchiveFormat" />
        /// </param>
        /// <param name="sourceDirectory">Path to package directory to archivate.</param>
        /// <param name="fileName">File name.</param>
        /// <param name="targetArchiveStream">Target stream to write archived data.</param>
        void Pack(ArchiveFormat archiveFormat, string sourceDirectory, string fileName, Stream targetArchiveStream);

        /// <summary>
        ///     Extracts archive to archive stream
        /// </summary>
        /// <param name="archiveFormat">
        ///     <see cref="ArchiveFormat" />
        /// </param>
        /// <param name="targetDirectory">Path to target directory where package will be extracted.</param>
        /// <param name="archiveStream"><see cref="Stream" /> of archivated package.</param>
        void Unpack(ArchiveFormat archiveFormat, string targetDirectory, Stream archiveStream);
    }
}