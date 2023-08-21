namespace Aristocrat.Monaco.Hardware.Serial.Protocols
{
    /// <summary>
    ///     Interface for a simple CRC engine
    /// </summary>
    public interface ICrcEngine
    {
        /// <summary>
        ///     Initialize to a starting value
        /// </summary>
        /// <param name="seed">Start value</param>
        void Initialize(ushort seed);

        /// <summary>
        ///     Hash (part of) a buffer.
        /// </summary>
        /// <param name="bytes">Buffer</param>
        /// <param name="start">Where in the buffer to start hashing</param>
        /// <param name="count">How many bytes to hash</param>
        void Hash(byte[] bytes, uint start, uint count);

        /// <summary>
        ///     Get the CRC
        /// </summary>
        ushort Crc { get; }
    }
}
