namespace Aristocrat.Monaco.Hhr.Client.Communication
{
    using System;

    /// <summary>
    ///     Encapsulation of data received from the network.
    /// </summary>
    public class Packet
    {
        /// <summary>
        ///     Constructor initializing buffer and length of buffer
        /// </summary>
        /// <param name="buffer">the bytes of the buffer</param>
        /// <param name="length">length of buffer data</param>
        public Packet(byte[] buffer, int length)
        {
            Data = new byte[length];
            Array.Copy(buffer, Data, length);
            Length = length;
        }

        /// <summary>
        ///     Length of data received.
        /// </summary>
        public int Length { get; }

        /// <summary>
        ///     Buffer received on Endpoint.
        /// </summary>
        public byte[] Data { get; }
    }
}