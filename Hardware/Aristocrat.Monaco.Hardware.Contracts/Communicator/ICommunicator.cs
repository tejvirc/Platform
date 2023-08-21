namespace Aristocrat.Monaco.Hardware.Contracts.Communicator
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using Gds;
    using SharedDevice;

    /// <summary>
    ///     Definition of the ICommunicator interface.
    /// </summary>
    public interface ICommunicator : IDisposable
    {
        /// <summary>
        ///     Gets the manufacturer.
        /// </summary>
        string Manufacturer { get; }

        /// <summary>
        ///     Gets the model.
        /// </summary>
        string Model { get; }

        /// <summary>
        ///     Gets the firmware.
        /// </summary>
        string Firmware { get; }

        /// <summary>
        ///     Gets the serial number.
        /// </summary>
        string SerialNumber { get; }

        /// <summary>
        ///     Gets a value indicating whether the port is open.
        /// </summary>
        /// <returns>True or false</returns>
        bool IsOpen { get; }

        /// <summary>
        ///     Gets the vendor ID.
        /// </summary>
        int VendorId { get; }

        /// <summary>
        ///     Gets the product ID.
        /// </summary>
        int ProductId { get; }

        /// <summary>
        ///     Gets the product ID DFU.
        /// </summary>
        int ProductIdDfu { get; }

        /// <summary>
        ///     Get the protocol.
        /// </summary>
        string Protocol { get; }

        /// <summary>
        ///     Get the device type.
        /// </summary>
        DeviceType DeviceType { get; set; }

        /// <summary>
        ///     Get or set the IDevice object to use for its serial port and settings.
        /// </summary>
        IDevice Device { get; set; }

        /// <summary>
        ///     Get the firmware version.
        /// </summary>
        string FirmwareVersion { get; }

        /// <summary>
        ///     Get the firmware CRC.
        /// </summary>
        int FirmwareCrc { get; }

        /// <summary>
        ///     Get whether the device is DFU capable.
        /// </summary>
        bool IsDfuCapable { get; }

        /// <summary>
        ///     Event fired when device is attached to communication channel.
        /// </summary>
        event EventHandler<EventArgs> DeviceAttached;

        /// <summary>
        ///     Event fired when device is detached from communication channel.
        /// </summary>
        event EventHandler<EventArgs> DeviceDetached;

        /// <summary>
        ///     Close the communication channel.
        /// </summary>
        /// <returns>True if it succeeds, false if it fails.</returns>
        bool Close();

        /// <summary>
        ///     Open communication port.
        /// </summary>
        /// <returns>True if it succeeds, false if it fails.</returns>
        bool Open();

        /// <summary>
        ///     Configure the communication channel.
        /// </summary>
        /// <param name="comConfiguration">The configuration to use.</param>
        /// <returns>True if it succeeds, false if it fails.</returns>
        bool Configure(IComConfiguration comConfiguration);

        /// <summary>Resets the connection.</summary>
        void ResetConnection();
    }
}
