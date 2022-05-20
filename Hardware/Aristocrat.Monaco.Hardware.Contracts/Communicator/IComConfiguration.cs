namespace Aristocrat.Monaco.Hardware.Contracts.Communicator
{
    using System.IO.Ports;

    /// <summary>
    ///     Definition of the IComConfiguration interface.
    /// </summary>
    public interface IComConfiguration
    {
        /// <summary>
        ///     Gets or sets communications mode.
        /// </summary>
        string Mode { get; set; }

        /// <summary>
        ///     Gets or sets communication protocol.
        /// </summary>
        string Protocol { get; set; }

        /// <summary>
        ///     Gets or sets baud rate.
        /// </summary>
        int BaudRate { get; set; }

        /// <summary>
        ///     Gets or sets data bits.
        /// </summary>
        int DataBits { get; set; }

        /// <summary>
        ///     Gets or sets handshake.
        /// </summary>
        Handshake Handshake { get; set; }

        /// <summary>
        ///     Gets or sets parity.
        /// </summary>
        Parity Parity { get; set; }

        /// <summary>
        ///     Gets or sets port name.
        /// </summary>
        string PortName { get; set; }

        /// <summary>
        ///     Gets or sets device name.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        ///     Gets or sets number of stop bits.
        /// </summary>
        StopBits StopBits { get; set; }

        /// <summary>
        ///     Gets or sets read buffer size.
        /// </summary>
        int ReadBufferSize { get; set; }

        /// <summary>
        ///     Gets or sets write buffer size.
        /// </summary>
        int WriteBufferSize { get; set; }

        /// <summary>
        ///     Gets or sets read timeout in milliseconds.
        /// </summary>
        int ReadTimeoutMs { get; set; }

        /// <summary>
        ///     Gets or sets write timeout in milliseconds.
        /// </summary>
        int WriteTimeoutMs { get; set; }

        /// <summary>
        ///     Gets or sets channel keep-alive timeout in milliseconds.
        /// </summary>
        int KeepAliveTimeoutMs { get; set; }

        /// <summary>
        ///     Gets or sets USB class GUID.
        /// </summary>
        string UsbClassGuid { get; set; }

        /// <summary>
        ///     Gets or sets USB device vendor id.
        /// </summary>
        string UsbDeviceVendorId { get; set; }

        /// <summary>
        ///     Gets or sets USB device product id.
        /// </summary>
        string UsbDeviceProductId { get; set; }

        /// <summary>
        ///     Gets or sets USB device product id for DFU.
        /// </summary>
        string UsbDeviceProductIdDfu { get; set; }
    }
}