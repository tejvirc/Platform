namespace Aristocrat.Monaco.Protocol.Common.Installer
{
    using System;
    using System.IO;
    using ICSharpCode.SharpZipLib.Zip;

    /// <summary>
    ///     ZIP archive implementation.
    /// </summary>
    public class ZipArchive : IZipArchive
    {
        /// <summary>
        ///     Creates archive in target archive stream from source directory
        /// </summary>
        /// <param name="sourceDirectory">Path to directory to archivate.</param>
        /// <param name="targetArchiveStream">Target stream.</param>
        public void Pack(string sourceDirectory, Stream targetArchiveStream)
        {
            if (sourceDirectory == null)
            {
                throw new ArgumentNullException(nameof(sourceDirectory));
            }

            if (targetArchiveStream == null)
            {
                throw new ArgumentNullException(nameof(targetArchiveStream));
            }

            using (var archiveStream = new MemoryStream())
            {
                var fastZip = new FastZip();
                fastZip.CreateZip(archiveStream, sourceDirectory, true, null, null);

                archiveStream.Copy(targetArchiveStream);
            }
        }

        /// <summary>
        ///     Creates archive in target archive stream from source directory
        /// </summary>
        /// <param name="sourceDirectory">Path to directory to archivate.</param>
        /// <param name="fileName">File name.</param>
        /// <param name="targetArchiveStream">Target stream.</param>
        public void Pack(string sourceDirectory, string fileName, Stream targetArchiveStream)
        {
            if (sourceDirectory == null)
            {
                throw new ArgumentNullException(nameof(sourceDirectory));
            }

            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            if (targetArchiveStream == null)
            {
                throw new ArgumentNullException(nameof(targetArchiveStream));
            }

            using (var archiveStream = new MemoryStream())
            {
                var fastZip = new FastZip();
                fastZip.CreateZip(archiveStream, sourceDirectory, true, fileName, null);

                archiveStream.Copy(targetArchiveStream);
            }
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

            var fastZip = new FastZip();
            fastZip.ExtractZip(archiveStream, targetDirectory, FastZip.Overwrite.Always, null, null, null, true, false);
        }
    }
}
