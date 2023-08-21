namespace Aristocrat.G2S.Client.Communications
{
    using System.IO;
    using System.IO.Compression;
    using System.Text;

    /// <summary>
    ///     A set of Gzip utilities
    /// </summary>
    public static class GzipUtilities
    {
        private static readonly Encoding Encoding = Encoding.UTF8;

        /// <summary>
        ///     Compresses (zips) the provided string
        /// </summary>
        /// <param name="text">The string to compress</param>
        /// <returns>the compressed string</returns>
        public static byte[] Zip(string text)
        {
            var bytes = Encoding.GetBytes(text);

            var output = new MemoryStream();

            using (var input = new MemoryStream(bytes))
            {
                using (var zip = new GZipStream(output, CompressionMode.Compress))
                {
                    input.CopyTo(zip);
                }

                return output.ToArray();
            }
        }

        /// <summary>
        ///     Decompresses (unzips) the provided buffer
        /// </summary>
        /// <param name="buffer">The buffer to decompress</param>
        /// <returns>the uncompressed buffer</returns>
        public static string Unzip(byte[] buffer)
        {
            using (var zipStream = new GZipStream(new MemoryStream(buffer), CompressionMode.Decompress))
            using (var resultStream = new MemoryStream())
            {
                zipStream.CopyTo(resultStream);

                return Encoding.GetString(resultStream.ToArray());
            }
        }
    }
}