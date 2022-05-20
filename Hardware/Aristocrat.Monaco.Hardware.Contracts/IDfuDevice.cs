namespace Aristocrat.Monaco.Hardware.Contracts
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Communicator;

    /// <summary>Interface for DFU device.</summary>
    public interface IDfuDevice : IDisposable
    {
        /// <summary>
        ///     Gets the identifier of the vendor.
        /// </summary>
        /// <value>The identifier of the vendor.</value>
        int VendorId { get; }

        /// <summary>
        ///     Gets the identifier of the product.
        /// </summary>
        /// <value>The identifier of the product.</value>
        int ProductId { get; }

        /// <summary>
        ///     Gets a value indicating whether the device is DFU capable.
        /// </summary>
        /// <returns>True or false</returns>
        bool IsDfuCapable { get; }

        /// <summary>
        ///     Check if device firmware download or upload is in progress.
        /// </summary>
        /// <returns> True if download or upload is in progress.</returns>
        bool IsDfuInProgress { get; }

        /// <summary>Occurs when download progresses.</summary>
        event EventHandler<ProgressEventArgs> DownloadProgressed;

        /// <summary>Initializes this device.</summary>
        /// <param name="communicator">The communicator.</param>
        /// <returns>An asynchronous result that yields true if it succeeds, false if it fails.</returns>
        Task<bool> Initialize(IGdsCommunicator communicator);

        /// <summary>Initiates detach sequence and enters DFU mode.</summary>
        /// <returns>True if it succeeds, false if it fails.</returns>
        Task<bool> Detach();

        /// <summary>If in DFU mode, exits the mode and reconnects the device.</summary>
        /// <returns>True if it succeeds, false if it fails.</returns>
        Task<bool> Reconnect();

        /// <summary>Request firmware download.</summary>
        /// <param name="firmware">Firmware stream.</param>
        /// <returns>An asynchronous result that yields true if it succeeds, false if it fails.</returns>
        Task<DfuStatus> Download(Stream firmware);

        /// <summary>Request firmware upload.</summary>
        /// <param name="firmware">Firmware stream.</param>
        /// <returns>An asynchronous result that yields true if it succeeds, false if it fails.</returns>
        Task<DfuStatus> Upload(Stream firmware);

        /// <summary>
        ///     Aborts the currently in progress download or upload operation.
        /// </summary>
        void Abort();
    }
}
