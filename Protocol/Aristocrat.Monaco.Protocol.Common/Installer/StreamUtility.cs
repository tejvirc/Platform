namespace Aristocrat.Monaco.Protocol.Common.Installer
{ 
    using System;
    using System.IO;

    /// <summary>
    ///     Stream helper.
    /// </summary>
    public static class StreamUtility
    {
        /// <summary>
        ///     Copy stream bytes of disposed strean to a new <see cref="MemoryStream" />.
        /// </summary>
        /// <param name="disposedStream">The disposed stream</param>
        /// <returns>a mempory stream</returns>
        public static MemoryStream CopyFrom(MemoryStream disposedStream)
        {
            if (disposedStream == null)
            {
                throw new ArgumentNullException(nameof(disposedStream));
            }

            var bytesArray = disposedStream.ToArray();

            var outStream = new MemoryStream();
            outStream.Write(bytesArray, 0, bytesArray.Length);

            return outStream;
        }

        /// <summary>
        ///     Copies data from disposed memory stream to specified target stream.
        /// </summary>
        /// <param name="disposedStream">Disposed memory stream.</param>
        /// <param name="targetStream">Target stream.</param>
        public static void Copy(this MemoryStream disposedStream, Stream targetStream)
        {
            if (disposedStream == null)
            {
                throw new ArgumentNullException(nameof(disposedStream));
            }

            var bytesArray = disposedStream.ToArray();
            targetStream.Write(bytesArray, 0, bytesArray.Length);
        }
    }
}