namespace Aristocrat.Mgam.Client.Routing
{
    using System.IO;
    using System.Threading.Tasks;
    using ICSharpCode.SharpZipLib.BZip2;

    /// <summary>
    ///     Compresses or decompresses message content using BZip2 compression.
    /// </summary>
    internal class BZip2Compressor : ICompressor
    {
        /// <inheritdoc />
        public async Task<byte[]> Compress(byte[] bytes)
        {
            using (var inStream = new MemoryStream(bytes))
                using (var outStream = new MemoryStream())
                {
                    BZip2.Compress(inStream, outStream, false, 9);

                    return await Task.FromResult(outStream.ToArray());
                }
        }

        /// <inheritdoc />
        public async Task<byte[]> Decompress(byte[] bytes)
        {
            using (var inStream = new MemoryStream(bytes))
                using (var outStream = new MemoryStream())
                {
                    BZip2.Decompress(inStream, outStream, false);

                    return await Task.FromResult(outStream.ToArray());
                }
        }
    }
}
