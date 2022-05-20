namespace Aristocrat.Monaco.Protocol.Common.Installer
{
    using System;
    using System.IO;

    /// <summary>
    ///     Package service implementation.
    /// </summary>
    public class PackageService : IPackageService
    {
        private readonly ITarArchive _tar;
        private readonly IZipArchive _zip;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PackageService" /> class.
        /// </summary>
        /// <param name="zip">ZIP archive implementation.</param>
        /// <param name="tar">TAR archive implementation.</param>
        public PackageService(IZipArchive zip, ITarArchive tar)
        {
            _zip = zip;
            _tar = tar;
        }

        /// <inheritdoc />
        public void Pack(ArchiveFormat archiveFormat, string sourceDirectory, Stream targetArchiveStream)
        {
            if (string.IsNullOrEmpty(sourceDirectory))
            {
                throw new ArgumentNullException(nameof(sourceDirectory));
            }

            switch (archiveFormat)
            {
                case ArchiveFormat.Zip:
                    _zip.Pack(sourceDirectory, targetArchiveStream);
                    break;
                case ArchiveFormat.Tar:
                    _tar.Pack(sourceDirectory, targetArchiveStream);
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <inheritdoc />
        public void Pack(
            ArchiveFormat archiveFormat,
            string sourceDirectory,
            string fileName,
            Stream targetArchiveStream)
        {
            if (string.IsNullOrEmpty(sourceDirectory))
            {
                throw new ArgumentNullException(nameof(sourceDirectory));
            }

            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            switch (archiveFormat)
            {
                case ArchiveFormat.Zip:
                    _zip.Pack(sourceDirectory, fileName, targetArchiveStream);
                    break;
                case ArchiveFormat.Tar:
                    _tar.Pack(sourceDirectory, fileName, targetArchiveStream);
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <inheritdoc />
        public void Unpack(ArchiveFormat archiveFormat, string targetDirectory, Stream archiveStream)
        {
            if (archiveStream == null)
            {
                throw new ArgumentNullException(nameof(archiveStream));
            }

            if (string.IsNullOrEmpty(targetDirectory))
            {
                throw new ArgumentNullException(nameof(targetDirectory));
            }

            switch (archiveFormat)
            {
                case ArchiveFormat.Zip:
                    _zip.Unpack(targetDirectory, archiveStream);
                    break;
                case ArchiveFormat.Tar:
                    _tar.Unpack(targetDirectory, archiveStream);
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}