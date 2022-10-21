namespace Aristocrat.Monaco.Hardware.Contracts.SharedDevice
{
    using System;
    using Communicator;

    /// <summary>
    ///     Expand on <see cref="ISerialPort"/>
    /// </summary>
    public interface ISerialPortController : ISerialPort
    {
        /// <summary>
        ///     Data has been received on the port.
        /// </summary>
        event EventHandler ReceivedData;

        /// <summary>
        ///     Error has been received on the port.
        /// </summary>
        event EventHandler ReceivedError;

        /// <summary>
        ///     The keep-alive timer expired during a receive operation.
        /// </summary>
        event EventHandler KeepAliveExpired;

        /// <summary>
        ///      Gets or sets a value indicating if the sync communication mode is used.
        /// </summary>
        bool UseSyncMode { get; set; }

        /// <summary>
        ///     Configure the port
        /// </summary>
        /// <param name="comConfig">Configuration details</param>
        void Configure(IComConfiguration comConfig);

        /// <summary>
        ///     Get or set a value indicating whether or not the port is enabled
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        ///     Write a byte buffer to the port.
        /// </summary>
        /// <param name="buffer">Buffer to send.</param>
        /// <returns>Success</returns>
        bool WriteBuffer(byte[] buffer);

        /// <summary>
        ///     Try to read into a buffer from the port.
        /// </summary>
        /// <param name="buffer">Buffer to read into</param>
        /// <param name="offset">Starting offset within buffer</param>
        /// <param name="count">How many bytes to read</param>
        /// <returns>Success</returns>
        int TryReadBuffer(ref byte[] buffer, int offset, int count);

        /// <summary>
        ///     Flush any remaining bytes in the port's receive buffer.
        /// </summary>
        void FlushInputAndOutput();
    }
}
