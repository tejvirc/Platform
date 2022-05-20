namespace Aristocrat.Monaco.Hardware.Contracts.SharedDevice
{
    using System;
    using System.IO;
    using System.IO.Ports;
    using System.Text;

    /// <summary>
    ///     Definition of the ISerialPort interface.
    /// </summary>
    public interface ISerialPort
    {
        /// <summary>
        ///     Gets base stream.
        /// </summary>
        Stream BaseStream { get; }

        /// <summary>
        ///     Gets or sets baud rate.
        /// </summary>
        int BaudRate { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether port is in break state.
        /// </summary>
        bool BreakState { get; set; }

        /// <summary>
        ///     Gets bytes to write.
        /// </summary>
        int BytesToWrite { get; }

        /// <summary>
        ///     Gets bytes to read.
        /// </summary>
        int BytesToRead { get; }

        /// <summary>
        ///     Gets a value indicating whether port is in CD holding state.
        /// </summary>
        bool CDHolding { get; }

        /// <summary>
        ///     Gets a value indicating whether port is in CTS holding state.
        /// </summary>
        bool CtsHolding { get; }

        /// <summary>
        ///     Gets or sets data bits.
        /// </summary>
        int DataBits { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to dicard null.
        /// </summary>
        bool DiscardNull { get; set; }

        /// <summary>
        ///     Gets a value indicating whether port is in Dsr holding state.
        /// </summary>
        bool DsrHolding { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether Dtr is enabled.
        /// </summary>
        bool DtrEnable { get; set; }

        /// <summary>
        ///     Gets or sets encoding.
        /// </summary>
        Encoding Encoding { get; set; }

        /// <summary>
        ///     Gets or sets handshake.
        /// </summary>
        Handshake Handshake { get; set; }

        /// <summary>
        ///     Gets a value indicating whether port is open.
        /// </summary>
        bool IsOpen { get; }

        /// <summary>
        ///     Gets or sets the value used to interpret the end of a call
        ///     to the ReadLine and WriteLine methods.
        /// </summary>
        string NewLine { get; set; }

        /// <summary>
        ///     Gets or sets parity.
        /// </summary>
        Parity Parity { get; set; }

        /// <summary>
        ///     Gets or sets the byte that replaces invalid bytes in a data stream
        ///     when a parity error occurs.
        /// </summary>
        byte ParityReplace { get; set; }

        /// <summary>
        ///     Gets or sets port name.
        /// </summary>
        string PortName { get; set; }

        /// <summary>
        ///     Gets or sets read buffer size.
        /// </summary>
        int ReadBufferSize { get; set; }

        /// <summary>
        ///     Gets or sets the number of milliseconds before a time-out occurs
        ///     when a read operation does not complete.
        /// </summary>
        int ReadTimeout { get; set; }

        /// <summary>
        ///     Gets or sets the number of bytes in the internal input buffer
        ///     before a DataReceived event occurs.
        /// </summary>
        int ReceivedBytesThreshold { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether Rts is enabled.
        /// </summary>
        bool RtsEnable { get; set; }

        /// <summary>
        ///     Gets or sets number of stop bits.
        /// </summary>
        StopBits StopBits { get; set; }

        /// <summary>
        ///     Gets or sets the size of the serial port output buffer.
        /// </summary>
        int WriteBufferSize { get; set; }

        /// <summary>
        ///     Gets or sets the number of milliseconds before a time-out occurs
        ///     when a write operation does not complete.
        /// </summary>
        int WriteTimeout { get; set; }

        /// <summary>
        ///     Event error received.
        /// </summary>
        event SerialErrorReceivedEventHandler ErrorReceived;

        /// <summary>
        ///     Event serial pin changed.
        /// </summary>
        event SerialPinChangedEventHandler PinChanged;

        /// <summary>
        ///     Event data received.
        /// </summary>
        event SerialDataReceivedEventHandler DataReceived;

        /// <summary>
        ///     Event disposed.
        /// </summary>
        event EventHandler Disposed;

        /// <summary>
        ///     Opens the serial port.
        /// </summary>
        void Open();

        /// <summary>
        ///     Closes the serial port.
        /// </summary>
        void Close();

        /// <summary>
        ///     Discards contents of the in buffer.
        /// </summary>
        void DiscardInBuffer();

        /// <summary>
        ///     Discards contents of the out buffer.
        /// </summary>
        void DiscardOutBuffer();

        /// <summary>
        ///     Reads a number of bytes from the SerialPort input buffer and writes
        ///     those bytes into a byte array at the specified offset.
        /// </summary>
        /// <param name="buffer">The byte array to write the input to.</param>
        /// <param name="offset">The offset in the buffer array to begin writing.</param>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>Number of bytes read.</returns>
        int Read(byte[] buffer, int offset, int count);

        /// <summary>
        ///     Synchronously reads one character from the SerialPort input buffer.
        /// </summary>
        /// <returns>Number of bytes read.</returns>
        int ReadChar();

        /// <summary>
        ///     Reads a number of characters from the SerialPort input buffer and writes
        ///     those characters into a char array at the specified offset.
        /// </summary>
        /// <param name="buffer">The char array to write the input to.</param>
        /// <param name="offset">The offset in the buffer array to begin writing.</param>
        /// <param name="count">The number of chars to read.</param>
        /// <returns>Number of bytes read.</returns>
        int Read(char[] buffer, int offset, int count);

        /// <summary>
        ///     Synchronously reads one byte from the SerialPort input buffer.
        /// </summary>
        /// <returns>Number of bytes read.</returns>
        int ReadByte();

        /// <summary>
        ///     Reads all immediately available bytes, based on the encoding,
        ///     in both the stream and the input buffer of the SerialPort object.
        /// </summary>
        /// <returns>String read.</returns>
        string ReadExisting();

        /// <summary>
        ///     Reads up to the NewLine value in the input buffer.
        /// </summary>
        /// <returns>String read.</returns>
        string ReadLine();

        /// <summary>
        ///     Reads a string up to the specified value in the input buffer.
        /// </summary>
        /// <param name="value">A value that indicates where the read operation stops.</param>
        /// <returns>The contents of the input buffer up to the specified value.</returns>
        string ReadTo(string value);

        /// <summary>
        ///     Writes the specified string to the serial port.
        /// </summary>
        /// <param name="text">The string to write.</param>
        void Write(string text);

        /// <summary>
        ///     Writes a specified number of chars to the serial port.
        /// </summary>
        /// <param name="buffer">The char array to write from.</param>
        /// <param name="offset">The offset in the buffer parameter at which to begin copying chars to the port. </param>
        /// <param name="count">The number of chars to write. </param>
        void Write(char[] buffer, int offset, int count);

        /// <summary>
        ///     Writes a specified number of bytes to the serial port.
        /// </summary>
        /// <param name="buffer">The byte array to write from.</param>
        /// <param name="offset">The offset in the buffer parameter at which to begin copying bytes to the port. </param>
        /// <param name="count">The number of bytes to write. </param>
        void Write(byte[] buffer, int offset, int count);

        /// <summary>
        ///     Writes the specified string and the NewLine value to the output buffer.
        /// </summary>
        /// <param name="text">The string to write to the output buffer.</param>
        void WriteLine(string text);

        /// <summary>
        ///     Dispose of the object.
        /// </summary>
        void Dispose();

        /// <summary>
        ///     Convert to String.
        /// </summary>
        /// <returns>String representation of this object.</returns>
        string ToString();
    }
}