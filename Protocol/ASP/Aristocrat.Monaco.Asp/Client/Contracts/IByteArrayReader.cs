namespace Aristocrat.Monaco.Asp.Client.Contracts
{
    /// <summary>
    ///     Interface to read byte array in big-endian format.
    /// </summary>
    public interface IByteArrayReader
    {
        /// <summary>
        ///     Reads one byte from current position in the array.
        /// </summary>
        /// <returns>the byte read.</returns>
        byte ReadByte();

        /// <summary>
        ///     Reads one 16 bit integer from current position in the array.
        /// </summary>
        /// <returns>the 16 bit integer read.</returns>
        short ReadInt16();

        /// <summary>
        ///     Reads one 32 bit integer from current position in the array.
        /// </summary>
        /// <returns>the 32 bit integer read.</returns>
        int ReadInt32();

        /// <summary>
        ///     Reads one string of specified size from current position in the array.
        /// </summary>
        /// <returns>the string read.</returns>
        string ReadString(int size);

        /// <summary>
        ///     Reads one 16 bit unsigned integer from current position in the array.
        /// </summary>
        /// <returns>the 16 bit unsigned integer read.</returns>
        ushort ReadUInt16();

        /// <summary>
        ///     Reads one 32 bit unsigned integer from current position in the array.
        /// </summary>
        /// <returns>the 32 bit unsigned integer read.</returns>
        uint ReadUInt32();

        /// <summary>
        ///     Reads one 32 bit float from current position in the array.
        /// </summary>
        /// <returns>the 32 bit float read.</returns>
        float ReadFloat();

        /// <summary>
        ///     Reads given number of bytes from current position in the array.
        /// </summary>
        /// <returns>the bytes read.</returns>
        byte[] ReadBytes(int size);
    }
}