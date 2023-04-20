namespace Aristocrat.Monaco.Hardware.Contracts.SharedDevice
{
    using Communicator;
    using Kernel;
    using SerialPorts;

    /// <summary>
    ///     Class used to describe a device.
    /// </summary>
    public sealed class Device : SerialPortController, IDevice
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Device" /> class.
        /// </summary>
        public Device()
            : this(ServiceManager.GetInstance().GetService<ISerialPortsService>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Device" /> class.
        /// </summary>
        public Device(ISerialPortsService serialPortsService)
            : this(null, null, serialPortsService)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Device" /> class.
        /// </summary>
        /// <param name="deviceConfiguration">The device configuration.</param>
        /// <param name="comConfiguration">The com configuration.</param>
        public Device(IDeviceConfiguration deviceConfiguration, IComConfiguration comConfiguration)
            : this(
                deviceConfiguration,
                comConfiguration,
                ServiceManager.GetInstance().GetService<ISerialPortsService>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Device" /> class.
        /// </summary>
        /// <param name="deviceConfiguration">The device configuration.</param>
        /// <param name="comConfiguration">The com configuration.</param>
        /// <param name="serialPortsService">An instance of <see cref="ISerialPortsService"/></param>
        public Device(
            IDeviceConfiguration deviceConfiguration,
            IComConfiguration comConfiguration,
            ISerialPortsService serialPortsService) : base(serialPortsService)
        {
            if (deviceConfiguration != null)
            {
                Protocol = deviceConfiguration.Protocol;

                Manufacturer = deviceConfiguration.Manufacturer;
                Model = deviceConfiguration.Model;
                SerialNumber = deviceConfiguration.SerialNumber;
                FirmwareBootVersion = deviceConfiguration.FirmwareBootVersion;
                FirmwareId = deviceConfiguration.FirmwareId;
                FirmwareRevision = deviceConfiguration.FirmwareRevision;
                FirmwareCyclicRedundancyCheck = deviceConfiguration.FirmwareCyclicRedundancyCheck;
                VariantName = deviceConfiguration.VariantName;
                VariantVersion = deviceConfiguration.VariantVersion;
                PollingFrequency = deviceConfiguration.PollingFrequency;
            }

            if (comConfiguration != null)
            {
                Configure(comConfiguration);

                Mode = comConfiguration.Mode;
            }
        }

        /// <summary>
        ///     Gets or sets a protocol value.
        /// </summary>
        public string Protocol { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets a manufacturer value.
        /// </summary>
        public string Manufacturer { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets a model value.
        /// </summary>
        public string Model { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets a serial number value.
        /// </summary>
        public string SerialNumber { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets a firmware bootstrap version.
        /// </summary>
        public string FirmwareBootVersion { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets a firmware id.
        /// </summary>
        public string FirmwareId { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets a firmware revision.
        /// </summary>
        public string FirmwareRevision { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets a firmware cyclic redundancy check.
        /// </summary>
        public string FirmwareCyclicRedundancyCheck { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets a variant name.
        /// </summary>
        public string VariantName { get; set; }  = string.Empty;

        /// <summary>
        ///     Gets or sets a variant version.
        /// </summary>
        public string VariantVersion { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets a polling frequency.
        /// </summary>
        public int PollingFrequency { get; set; }

        /// <summary>
        ///     Gets or sets the mode.
        /// </summary>
        public string Mode { get; set; }
    }
}