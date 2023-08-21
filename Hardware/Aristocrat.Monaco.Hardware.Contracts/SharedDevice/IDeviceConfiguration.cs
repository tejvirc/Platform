namespace Aristocrat.Monaco.Hardware.Contracts.SharedDevice
{
    /// <summary>
    ///     Definition of the IDeviceConfiguration interface.
    /// </summary>
    public interface IDeviceConfiguration
    {
        /// <summary>
        ///     Gets or sets firmware boot version
        /// </summary>
        string FirmwareBootVersion { get; set; }

        /// <summary>
        ///     Gets or sets cyclic redundancy check value
        /// </summary>
        string FirmwareCyclicRedundancyCheck { get; set; }

        /// <summary>
        ///     Gets or sets firmware id
        /// </summary>
        string FirmwareId { get; set; }

        /// <summary>
        ///     Gets or sets firmware revision
        /// </summary>
        string FirmwareRevision { get; set; }

        /// <summary>
        ///     Gets or sets manufacturer
        /// </summary>
        string Manufacturer { get; set; }

        /// <summary>
        ///     Gets or sets model
        /// </summary>
        string Model { get; set; }

        /// <summary>
        ///     Gets or sets a polling frequency
        /// </summary>
        int PollingFrequency { get; set; }

        /// <summary>
        ///     Gets or sets a protocol value
        /// </summary>
        string Protocol { get; set; }

        /// <summary>
        ///     Gets or sets firmware revision
        /// </summary>
        string SerialNumber { get; set; }

        /// <summary>
        ///     Gets or sets a variant name
        /// </summary>
        string VariantName { get; set; }

        /// <summary>
        ///     Gets or sets a variant version
        /// </summary>
        string VariantVersion { get; set; }
    }
}