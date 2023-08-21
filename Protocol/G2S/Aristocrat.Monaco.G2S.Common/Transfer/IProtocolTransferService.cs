namespace Aristocrat.Monaco.G2S.Common.Transfer
{
    using System.IO;
    using System.Threading;

    /// <summary>
    ///     Base interface for transfer service that allows to upload or download big files from Software Download Distribution
    ///     Point that could be FTP, Cloud or something else.
    /// </summary>
    public interface IProtocolTransferService
    {
        /// <summary>
        ///     Uploads specified source stream to Software Download Distribution Point.
        /// </summary>
        /// <param name="destinationLocation">An IP address:port/path/filename or valid network URI address.</param>
        /// <param name="transferParameters">A string containing optional parameters required for the transfer of the package.</param>
        /// <param name="sourceStream">Source stream that should be uploaded to destination</param>
        /// <param name="ct">The cancellation token.</param>
        void Upload(
            string destinationLocation,
            string transferParameters,
            Stream sourceStream,
            CancellationToken ct);

        /// <summary>
        ///     Downloads specified package into stream from Software Download Distribution Point.
        /// </summary>
        /// <param name="downloadLocation">An IP address:port/path/filename or valid network URI address.</param>
        /// <param name="transferParameters">A string containing optional parameters required for the transfer of the package.</param>
        /// <param name="destinationStream">Destination stream that should contain downloaded content.</param>
        /// <param name="ct">The cancellation token.</param>
        void Download(
            string downloadLocation,
            string transferParameters,
            Stream destinationStream,
            CancellationToken ct);
    }
}