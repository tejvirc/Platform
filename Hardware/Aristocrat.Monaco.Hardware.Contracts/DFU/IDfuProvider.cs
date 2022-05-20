namespace Aristocrat.Monaco.Hardware.Contracts.Dfu
{
    using System.Threading.Tasks;
    using Kernel;

    /// <summary>
    ///     Interface for DFU provider functionality.
    /// </summary>
    public interface IDfuProvider : IService
    {
        /// <summary>Gets a value indicating whether the initialized.</summary>
        /// <value>True if initialized, false if not.</value>
        bool Initialized { get; }

        /// <summary>Query if 'vendorId' is download in progress.</summary>
        /// <param name="vendorId"> Identifier for the vendor.</param>
        /// <param name="productId">Identifier for the product.</param>
        /// <returns>True if download in progress, false if not.</returns>
        bool IsDownloadInProgress(int vendorId, int productId);

        /// <summary>Downloads the filename described by vendorId.</summary>
        /// <param name="vendorId"> Identifier for the vendor.</param>
        /// <param name="productId">Identifier for the product.</param>
        /// <returns>A string.</returns>
        string DownloadFilename(int vendorId, int productId);

        /// <summary>
        ///     Downloads firmware to the device.
        /// </summary>
        /// <param name="file">Path to downloadable firmware file</param>
        /// <returns>Zero if it succeeds. Non-zero, otherwise.</returns>
        Task<bool> Download(string file);

        /// <summary>Aborts.</summary>
        /// <param name="vendorId"> Identifier for the vendor.</param>
        /// <param name="productId">Identifier for the product.</param>
        /// <returns>An asynchronous result.</returns>
        Task Abort(int vendorId, int productId);

        /// <summary>Registers a dfu adapter.</summary>
        /// <param name="adapter">The adapter.</param>
        void Register(IDfuDevice adapter);

        /// <summary>Unregisters a dfu adapter.</summary>
        /// <param name="adapter">The adapter.</param>
        void Unregister(IDfuDevice adapter);
    }
}