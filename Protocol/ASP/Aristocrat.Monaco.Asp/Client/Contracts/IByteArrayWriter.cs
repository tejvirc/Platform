namespace Aristocrat.Monaco.Asp.Client.Contracts
{
    /// <summary>
    ///     Interface to write byte array in big-endian format.
    /// </summary>
    public interface IByteArrayWriter
    {
        /// <summary>
        ///     Writes a byte into the byte array.
        /// </summary>
        /// <param name="value">The byte to write</param>
        void Write(byte value);

        /// <summary>
        ///     Writes one 32 bit integer at current position in the array.
        /// </summary>
        /// <param name="value">The 32 bit integer to write</param>
        void Write(int value);

        /// <summary>
        ///     Writes one 16 bit integer at current position in the array.
        /// </summary>
        /// <param name="value">The 32 bit integer to write</param>
        void Write(short value);

        /// <summary>
        ///     Writes a zero padded string of specified size at current position in the array. If string is larger than specified
        ///     size it is truncated with a null terminator.
        /// </summary>
        /// <param name="value">The string to write.</param>
        /// <param name="size">Size of bytes to write.</param>
        void Write(string value, int size);

        /// <summary>
        ///     Writes one 32 bit unsigned integer at current position in the array.
        /// </summary>
        /// <param name="value">The 32 bit unsigned integer to write</param>
        void Write(uint value);

        /// <summary>
        ///     Writes one 16 bit unsigned integer at current position in the array.
        /// </summary>
        /// <param name="value">The 16 bit unsigned integer to write</param>
        void Write(ushort value);

        /// <summary>
        ///     Writes one 32 bit float at current position in the array.
        /// </summary>
        /// <param name="value">The 32 bit float to write</param>
        void Write(float value);

        /// <summary>
        ///     Writes specified number of bytes at current position in the array.
        /// </summary>
        /// <param name="bytes">Bytes array to write.</param>
        /// <param name="offset">Offset in the source array starting from which to write bytes.</param>
        /// <param name="count">Number of bytes to write.</param>
        void Write(byte[] bytes, int offset = 0, int count = -1);
    }
}