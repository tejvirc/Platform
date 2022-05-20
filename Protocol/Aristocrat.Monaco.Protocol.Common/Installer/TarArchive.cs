namespace Aristocrat.Monaco.Protocol.Common.Installer
{
    using System;
    using System.IO;
    using Application.Contracts.Localization;
    using ICSharpCode.SharpZipLib.Tar;
    using Localization.Properties;

    /// <summary>
    ///     TAR archive implementation.
    /// </summary>
    public class TarArchive : ITarArchive
    {
        /// <summary>
        ///     Creates archive in target archive stream from source directory
        /// </summary>
        /// <param name="sourceDirectory">Path to directory to archivate.</param>
        /// <param name="targetArchiveStream">Target archive stream content.</param>
        public void Pack(string sourceDirectory, Stream targetArchiveStream)
        {
            if (string.IsNullOrEmpty(sourceDirectory))
            {
                throw new ArgumentNullException(nameof(sourceDirectory));
            }

            if (targetArchiveStream == null)
            {
                throw new ArgumentNullException(nameof(targetArchiveStream));
            }

            sourceDirectory = NormalizePath(sourceDirectory);

            var archiveStream = new MemoryStream();
            Stream tarStream = new TarOutputStream(archiveStream, 1);

            var tarArchive = ICSharpCode.SharpZipLib.Tar.TarArchive.CreateOutputTarArchive(tarStream);

            tarArchive.RootPath = sourceDirectory;

            AddDirectoryFilesToTar(tarArchive, sourceDirectory, true);

            tarArchive.Close();

            archiveStream.Copy(targetArchiveStream);
        }

        /// <summary>
        ///     Creates archive in target archive stream from source directory
        /// </summary>
        /// <param name="sourceDirectory">Path to directory to archivate.</param>
        /// <param name="fileName">File name.</param>
        /// <param name="targetArchiveStream">Target archive stream content.</param>
        public void Pack(string sourceDirectory, string fileName, Stream targetArchiveStream)
        {
            if (string.IsNullOrEmpty(sourceDirectory))
            {
                throw new ArgumentNullException(nameof(sourceDirectory));
            }

            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            if (targetArchiveStream == null)
            {
                throw new ArgumentNullException(nameof(targetArchiveStream));
            }

            sourceDirectory = NormalizePath(sourceDirectory);

            var archiveStream = new MemoryStream();
            Stream tarStream = new TarOutputStream(archiveStream, 1);

            var tarArchive = ICSharpCode.SharpZipLib.Tar.TarArchive.CreateOutputTarArchive(tarStream);

            tarArchive.RootPath = sourceDirectory;

            AddDirectoryFilesToTar(tarArchive, sourceDirectory, true, fileName);

            tarArchive.Close();

            archiveStream.Copy(targetArchiveStream);
        }

        /// <summary>
        ///     Extracts archive stream to target directory
        /// </summary>
        /// <param name="targetDirectory">Path to target directory where archive will be extracted.</param>
        /// <param name="archiveStream"><see cref="Stream" /> of archive.</param>
        public void Unpack(string targetDirectory, Stream archiveStream)
        {
            if (archiveStream == null)
            {
                throw new ArgumentNullException(nameof(archiveStream));
            }

            if (string.IsNullOrEmpty(targetDirectory))
            {
                throw new ArgumentNullException(nameof(targetDirectory));
            }

            if (archiveStream.CanSeek != true)
            {
                throw new ArgumentException(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.StreamNotSeekable), nameof(archiveStream.CanSeek));
            }

            targetDirectory = NormalizePath(targetDirectory);

            archiveStream.Seek(0, SeekOrigin.Begin);
            var tarArchive = ICSharpCode.SharpZipLib.Tar.TarArchive.CreateInputTarArchive(archiveStream);
            tarArchive.ExtractContents(targetDirectory);
            tarArchive.Close();
        }

        private static string NormalizePath(string path)
        {
            // Note that the RootPath is currently case sensitive and must be forward slashes e.g. "c:/temp"
            // and must not end with a slash, otherwise cuts off first char of filename
            // This is scheduled for fix in next release
            return
                path.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    .TrimEnd(Path.AltDirectorySeparatorChar);
        }

        private static void AddDirectoryFilesToTar(
            ICSharpCode.SharpZipLib.Tar.TarArchive tarArchive,
            string sourceDirectory,
            bool recurse,
            string targetName = null)
        {
            var tarEntry = TarEntry.CreateEntryFromFile(sourceDirectory);
            tarArchive.WriteEntry(tarEntry, false);

            var filenames = Directory.GetFiles(sourceDirectory);
            foreach (var filename in filenames)
            {
                if (targetName != null && !filename.Equals(targetName))
                {
                    continue;
                }

                tarEntry = TarEntry.CreateEntryFromFile(filename);
                tarArchive.WriteEntry(tarEntry, true);
            }

            if (recurse)
            {
                var directories = Directory.GetDirectories(sourceDirectory);
                foreach (var directory in directories)
                {
                    AddDirectoryFilesToTar(tarArchive, directory, true);
                }
            }
        }
    }
}