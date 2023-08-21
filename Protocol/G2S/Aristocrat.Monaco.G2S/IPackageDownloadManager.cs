namespace Aristocrat.Monaco.G2S
{
    using Aristocrat.G2S.Client.Devices;
    using Common.PackageManager.Storage;

    /// <summary>
    ///     Defines a contract for a package download manager.
    /// </summary>
    public interface IPackageDownloadManager
    {
        /// <summary>
        ///     Starts processing any waiting scripts.
        /// </summary>
        void Start();

        /// <summary>
        ///     Package download
        /// </summary>
        /// <param name="transferEntity">Transfer entity.</param>
        /// <param name="device">Download device.</param>
        void PackageDownload(TransferEntity transferEntity, IDownloadDevice device);
    }
}