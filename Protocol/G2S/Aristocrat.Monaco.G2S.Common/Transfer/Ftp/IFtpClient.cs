namespace Aristocrat.Monaco.G2S.Common.Transfer.Ftp
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     Base interface for FTP client implementation.
    /// </summary>
    public interface IFtpClient : IDisposable
    {
        /// <summary>
        ///     Gets current connect location address to be used for logs.
        /// </summary>
        string CurrentLocation { get; }

        /// <summary>
        ///     Gets current connect location address to be used for logs.
        /// </summary>
        string LocalFileLocation { get; }

        /// <summary>
        ///     Connects to FTP server.
        /// </summary>
        /// <param name="location">An IP address:port/path/filename or valid network URI address.</param>
        /// <param name="parameters">A string containing optional parameters required for the transfer of the package.</param>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        Task Connect(string location, string parameters);

        /// <summary>
        ///     Uploads specified source stream to Software Download Distribution Point.
        /// </summary>
        /// <param name="sourceStream">Source stream that should be uploaded to destination</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        Task Upload(Stream sourceStream, CancellationToken ct);

        /// <summary>
        ///     Downloads specified package into stream from Software Download Distribution Point.
        /// </summary>
        /// <param name="destinationStream">Destination stream that should contain downloaded content.</param>
        /// <param name="ct">CancellationToken</param>
        /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
        Task Download(Stream destinationStream, CancellationToken ct);

        /// <summary>
        ///     Checks either files exists or not.
        /// </summary>
        /// <param name="packageId">Uniquely identifies the package.</param>
        /// <param name="location">An IP address:port/path/filename or valid network URI address.</param>
        /// <returns>true if the file exists</returns>
        bool IsFileExists(string packageId, string location);

        /// <summary>
        ///     Deletes file.
        /// </summary>
        /// <param name="packageId">Uniquely identifies the package.</param>
        /// <param name="location">An IP address:port/path/filename or valid network URI address.</param>
        void DeleteFile(string packageId, string location);
    }
}