namespace Aristocrat.Monaco.Hardware.Contracts.SharedDevice
{
    using System.IO.Ports;
    using Communicator;
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
        ///     Initializes a new instance of the <see cref="Device" /> class.
        /// </summary>
        /// <param name="protocol">Protocol used by the device.</param>
        /// <param name="mode">Mode of communication used (e.g., RS232, USB).</param>
        /// <param name="portName">Communication port to which the device is connected.</param>
        /// <param name="baudRate">
        ///     Baud rate at which the device is connected.  The baud rate is the number
        ///     of times per second a serial communication signal changes states; a state being either a voltage
        ///     level, a frequency, or a frequency phase angle.
        /// </param>
        /// <param name="parity">
        ///     Parity is an optional parameter used in serial communications to
        ///     determine if the data character being transmitted is correctly received by the remote device.
        /// </param>
        /// <param name="dataBits">
        ///     Data bits indicates the number of bits used to represent
        ///     a single data character during serial communication.
        /// </param>
        /// <param name="stopBits">
        ///     Stop bits are used in asynchronous communication as a means of timing or
        ///     synchronizing the data characters being transmitted.
        /// </param>
        /// <param name="handshake">
        ///     Handshake is used to describe the method in which a serial device
        ///     controls the amount of data being transmitted to itself.
        /// </param>
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
        public Device(
            string protocol,
            string mode,
            string portName,
            int baudRate,
            Parity parity,
            int dataBits,
            StopBits stopBits,
            Handshake handshake,
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
            Configure(
                new ComConfiguration
                {
                    PortName = portName,
                    Mode = mode,
                    BaudRate = baudRate,
                    Parity = parity,
                    DataBits = dataBits,
                    StopBits = stopBits,
                    Handshake = handshake
                });

            Protocol = protocol;
            Mode = mode;
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
        ///     Initializes a new instance of the <see cref="Device" /> class.
        /// </summary>
        /// <param name="deviceConfiguration">The device configuration.</param>
        /// <param name="comConfiguration">The com configuration.</param>
        public Device(IDeviceConfiguration deviceConfiguration, IComConfiguration comConfiguration)
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
        ///     Gets or sets a firmware bootstrap version.
        /// </summary>
        public string FirmwareBootVersion { get; set; }

        /// <summary>
        ///     Gets or sets a firmware id.
        /// </summary>
        public string FirmwareId { get; set; }

        /// <summary>
        ///     Gets or sets a firmware revision.
        /// </summary>
        public string FirmwareRevision { get; set; }

        /// <summary>
        ///     Gets or sets a firmware cyclic redundancy check.
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
        ///     Gets or sets a polling frequency.
        /// </summary>
        public int PollingFrequency { get; set; }

        /// <summary>
        ///     Gets or sets the mode.
        /// </summary>
        public string Mode { get; set; }
    }
}