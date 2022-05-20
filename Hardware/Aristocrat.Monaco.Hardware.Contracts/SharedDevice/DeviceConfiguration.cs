namespace Aristocrat.Monaco.Hardware.Contracts.SharedDevice
{
    /// <summary>
    ///     Definition of the DeviceConfiguration class.
    /// </summary>
    public class DeviceConfiguration : IDeviceConfiguration
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DeviceConfiguration" /> class.
        /// </summary>
        public DeviceConfiguration()
        {
            Protocol = string.Empty;

            Manufacturer = string.Empty;
            Model = string.Empty;
            SerialNumber = string.Empty;
            FirmwareBootVersion = string.Empty;
            FirmwareId = string.Empty;
            FirmwareRevision = string.Empty;
            FirmwareCyclicRedundancyCheck = string.Empty;
            VariantName = string.Empty;
            VariantVersion = string.Empty;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DeviceConfiguration" /> class.
        /// </summary>
        /// <param name="protocol">Protocol used by the device.</param>
        /// <param name="manufacturer">Manufacturer of the connected device.</param>
        /// <param name="model">Model of the connected device.</param>
        /// <param name="serialNumber">Serial number of the connected device.</param>
        /// <param name="firmwareBootVersion">Boot version of the firmware installed on the connected device.</param>
        /// <param name="firmwareId">Firmware ID of firmware installed on the connected device.</param>
        /// <param name="firmwareRevision">Firmware revision of firmware installed on the connected device.</param>
        /// <param name="firmwareCyclicRedundancyCheck">Firmware CRC of firmware installed on the connected device.</param>
        /// <param name="variantName">Variant name installed on the connected device.</param>
        /// <param name="variantVersion">Variant version installed on the connected device.</param>
        /// <param name="pollingFrequency">Frequency in milliseconds at which the device is polled.</param>
        public DeviceConfiguration(
            string protocol,
            string manufacturer,
            string model,
            string serialNumber,
            string firmwareBootVersion,
            string firmwareId,
            string firmwareRevision,
            string firmwareCyclicRedundancyCheck,
            string variantName,
            string variantVersion,
            int pollingFrequency)
        {
            Protocol = protocol;

            Manufacturer = manufacturer;
            Model = model;
            SerialNumber = serialNumber;
            FirmwareBootVersion = firmwareBootVersion;
            FirmwareId = firmwareId;
            FirmwareRevision = firmwareRevision;
            FirmwareCyclicRedundancyCheck = firmwareCyclicRedundancyCheck;
            VariantName = variantName;
            VariantVersion = variantVersion;
            PollingFrequency = pollingFrequency;
        }

        /// <summary>
        ///     Gets or sets a protocol value.
        /// </summary>
        public string Protocol { get; set; }

        /// <summary>
        ///     Gets or sets a manufacturer value.
        /// </summary>
        public string Manufacturer { get; set; }

        /// <summary>
        ///     Gets or sets a model value.
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        ///     Gets or sets a serial number value.
        /// </summary>
        public string SerialNumber { get; set; }

        /// <summary>
        ///     Gets or sets firmware boot version.
        /// </summary>
        public string FirmwareBootVersion { get; set; }

        /// <summary>
        ///     Gets or sets firmware id.
        /// </summary>
        public string FirmwareId { get; set; }

        /// <summary>
        ///     Gets or sets firmware revision.
        /// </summary>
        public string FirmwareRevision { get; set; }

        /// <summary>
        ///     Gets or sets cyclic redundancy check value.
        /// </summary>
        public string FirmwareCyclicRedundancyCheck { get; set; }

        /// <summary>
        ///     Gets or sets a variant name.
        /// </summary>
        public string VariantName { get; set; }

        /// <summary>
        ///     Gets or sets a variant version.
        /// </summary>
        public string VariantVersion { get; set; }

        /// <summary>
        ///     Gets or sets a polling frequency
        /// </summary>
        public int PollingFrequency { get; set; }
    }
}