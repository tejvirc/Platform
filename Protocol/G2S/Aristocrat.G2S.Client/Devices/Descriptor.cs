namespace Aristocrat.G2S.Client.Devices
{
    /// <summary>
    ///     Contract for a device descriptor.  This should be used to provide information about USB devices that are GDS
    ///     compatible.
    /// </summary>
    public class Descriptor
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Descriptor" /> class
        /// </summary>
        /// <param name="vendorId">The USB vendor identifier</param>
        /// <param name="productId">The product identifier</param>
        /// <param name="releaseNumber">The release number</param>
        /// <param name="vendorName">The vendor name</param>
        /// <param name="productName">The product name</param>
        /// <param name="serialNumber">The serial number</param>
        public Descriptor(
            string vendorId,
            string productId,
            string releaseNumber,
            string vendorName,
            string productName,
            string serialNumber)
        {
            VendorId = vendorId;
            ProductId = productId;
            ReleaseNumber = releaseNumber;
            VendorName = vendorName;
            ProductName = productName;
            SerialNumber = serialNumber;
        }

        /// <summary>
        ///     Gets USB vendor identifier.
        /// </summary>
        public string VendorId { get; }

        /// <summary>
        ///     Gets product identifier.
        /// </summary>
        public string ProductId { get; }

        /// <summary>
        ///     Gets release number.
        /// </summary>
        public string ReleaseNumber { get; }

        /// <summary>
        ///     Gets the vendor name.
        /// </summary>
        public string VendorName { get; }

        /// <summary>
        ///     Gets the product name.
        /// </summary>
        public string ProductName { get; }

        /// <summary>
        ///     Gets the serial number.
        /// </summary>
        public string SerialNumber { get; }
    }
}