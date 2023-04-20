namespace Aristocrat.Monaco.Hardware.Contracts.Communicator
{
    using System;
    using System.IO.Ports;

    /// <summary>
    ///     Definition of the ComConfiguration class.
    /// </summary>
    public class ComConfiguration : IComConfiguration
    {
        /// <summary>
        ///     Indicates RS232 communication mode.
        /// </summary>
        public const string RS232CommunicationMode = "RS232";

        /// <summary>
        ///     Indicates USB communication mode.
        /// </summary>
        public const string USBCommunicationMode = "USB";

        /// <summary>
        ///     Indicates RELM communication protocol.
        /// </summary>
        public const string RelmProtocol = "RELM";

        /// <summary>
        ///     Constructor to set defaults.
        /// </summary>
        public ComConfiguration()
        {
            Mode = RS232CommunicationMode;
            Protocol = "Undefined";

            // RS-232 defaults
            BaudRate = 9600;
            DataBits = 8;
            Parity = Parity.None;
            StopBits = StopBits.One;
            Handshake = Handshake.None;
            ReadBufferSize = 4096;
            WriteBufferSize = 2048;
            ReadTimeoutMs = -1;
            WriteTimeoutMs = -1;
            KeepAliveTimeoutMs = -1;

            // USB defaults
            UsbClassGuid = Guid.Empty.ToString();
            UsbDeviceVendorId = "vid_0000";
            UsbDeviceProductId = "pid_0000";
            UsbDeviceProductIdDfu = "pid_0000";
        }

        /// <inheritdoc />
        public string Mode { get; set; }

        /// <inheritdoc />
        public string Protocol { get; set; }

        /// <inheritdoc />
        public string Name { get; set; }

        /// <inheritdoc />
        public int BaudRate { get; set; }

        /// <inheritdoc />
        public int DataBits { get; set; }

        /// <inheritdoc />
        public Handshake Handshake { get; set; }

        /// <inheritdoc />
        public Parity Parity { get; set; }

        /// <inheritdoc />
        public string PortName { get; set; }

        /// <inheritdoc />
        public StopBits StopBits { get; set; }

        /// <inheritdoc />
        public int ReadBufferSize { get; set; }

        /// <inheritdoc />
        public int WriteBufferSize { get; set; }

        /// <inheritdoc />
        public int ReadTimeoutMs { get; set; }

        /// <inheritdoc />
        public int WriteTimeoutMs { get; set; }

        /// <inheritdoc />
        public int KeepAliveTimeoutMs { get; set; }

        /// <inheritdoc />
        public string UsbClassGuid { get; set; }

        /// <inheritdoc />
        public string UsbDeviceVendorId { get; set; }

        /// <inheritdoc />
        public string UsbDeviceProductId { get; set; }

        /// <inheritdoc />
        public string UsbDeviceProductIdDfu { get; set; }

        /// <inheritdoc />
        public int MaxPollTimeouts { get; set; } = HardwareConstants.DefaultMaxFailedPollCount;
    }
}