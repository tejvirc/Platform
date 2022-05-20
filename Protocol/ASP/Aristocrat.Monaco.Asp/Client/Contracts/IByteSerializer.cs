namespace Aristocrat.Monaco.Asp.Client.Contracts
{
    /// <summary>
    ///     An interface to serialize into byte array in big-endian format.
    /// </summary>
    public interface IByteSerializer
    {
        /// <summary>
        ///     De-serializes the bytes into the object.
        /// </summary>
        /// <param name="reader">The byte array reader to read bytes from.</param>
        void ReadBytes(IByteArrayReader reader);

        /// <summary>
        ///     Serializes the object into bytes.
        /// </summary>
        /// <param name="writer">The byte array writer to write bytes to.</param>
        void WriteBytes(IByteArrayWriter writer);
    }
}