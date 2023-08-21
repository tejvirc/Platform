namespace Aristocrat.Mgam.Client.Routing
{
    using System.Threading.Tasks;

    /// <summary>
    ///     
    /// </summary>
    internal interface ICompressor
    {
        /// <summary>
        ///     
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        Task<byte[]> Compress(byte[] bytes);

        /// <summary>
        ///     
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        Task<byte[]> Decompress(byte[] bytes);
    }
}
