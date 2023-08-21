namespace Aristocrat.Monaco.G2S.Common.Transfer.Ftp
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     FTP implementation of transfer service that supports FTP, FTPS, FTPES and SFTP protocols.
    /// </summary>
    /// <remarks>
    ///     TBD: Additional parameters should specify what protocol should be used and other required information.
    /// </remarks>
    public class FtpTransferService : IProtocolTransferService
    {
        /// <inheritdoc />
        public void Upload(
            string destinationLocation,
            string transferParameters,
            Stream sourceStream,
            CancellationToken ct)
        {
            UploadAsync(destinationLocation, transferParameters, sourceStream, ct).Wait(ct);
        }

        /// <inheritdoc />
        public void Download(
            string downloadLocation,
            string transferParameters,
            Stream destinationStream,
            CancellationToken ct)
        {
            DownloadAsync(downloadLocation, transferParameters, destinationStream, ct).Wait(ct);
        }

        /// <summary>
        ///     Gets appropriate FTP client implementation based on location and transfer parameters.
        /// </summary>
        /// <param name="location">An IP address:port/path/filename or valid network URI address.</param>
        /// <returns>An ftp client instance.</returns>
        public virtual IFtpClient GetFtpClient(string location)
        {
            // According to G2S Download Class specification in "Transfer Protocol Undefined" section.
            // SFTP URI should follow to draft-ietf-secsh-scp-sftp-ssh-uri-04 format e.g. sftp://abc_mfg.com:115/downloadServer/game.package
            if (location.StartsWith("sftp", StringComparison.InvariantCultureIgnoreCase))
            {
                return new SecureFtpClient();
            }

            return new FtpClient();
        }

        private async Task DownloadAsync(
            string downloadLocation,
            string transferParameters,
            Stream destinationStream,
            CancellationToken ct)
        {
            using (var client = GetFtpClient(downloadLocation))
            {
                await client.Connect(downloadLocation, transferParameters);
                await client.Download(destinationStream, ct);
            }
        }

        private async Task UploadAsync(
            string destinationLocation,
            string transferParameters,
            Stream sourceStream,
            CancellationToken ct)
        {
            using (var client = GetFtpClient(destinationLocation))
            {
                await client.Connect(destinationLocation, transferParameters);
                await client.Upload(sourceStream, ct);
            }
        }
    }
}