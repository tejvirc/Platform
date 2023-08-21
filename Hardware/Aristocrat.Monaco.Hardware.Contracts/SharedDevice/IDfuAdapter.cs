namespace Aristocrat.Monaco.Hardware.Contracts.SharedDevice
{
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>Interface for dfu adapter.</summary>
    public interface IDfuAdapter
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

        /// <summary>Gets or sets the filename of the download file.</summary>
        /// <value>The filename of the download file.</value>
        string DownloadFilename { get; set; }

        /// <summary>Downloads firmware to the device.</summary>
        /// <returns>Zero if successful.Non-zero, if it fails.</returns>
        Task<bool> Download(Stream firmware);

        /// <summary>Aborts current download.</summary>
        /// <returns>True if it succeeds, false if it fails.</returns>
        Task Abort();
    }
}
