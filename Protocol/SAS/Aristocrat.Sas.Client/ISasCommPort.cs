namespace Aristocrat.Sas.Client
{
    using System.Collections.Generic;

    public interface ISasCommPort
    {
        /// <summary>
        ///     Gets or sets the SAS address associated with this comm port.
        ///     The SAS address is rarely different from 1.
        /// </summary>
        byte SasAddress { get; set; }

        /// <summary>
        ///     Attempts to open the given comm port
        /// </summary>
        /// <param name="port">The comm port name. For example "COM5"</param>
        /// <returns>true if the port opened</returns>
        bool Open(string port);

        /// <summary>
        ///     Closes the comm port
        /// </summary>
        void Close();

        /// <summary>
        ///     Sends the passed in bytes over the comm port.
        ///     Blocks while the bytes are being sent.
        /// </summary>
        /// <param name="bytesToSend">The bytes to send</param>
        /// <returns>true if all bytes sent without errors. False otherwise</returns>
        bool SendRawBytes(IReadOnlyCollection<byte> bytesToSend);

        /// <summary>
        /// Sends a chirp response.
        /// </summary>
        /// <returns>true if chirp sent ok. False otherwise</returns>
        bool SendChirp();

        /// <summary>
        ///     Read one byte directly from the comm port.
        ///     Blocks while waiting for the byte.
        /// </summary>
        /// <param name="isLongPoll">Indicates whether the byte to read belongs to a long poll.</param>
        /// <returns>
        /// A tuple with the following items:
        ///   1) - the byte to be received. it should be ignored if readStatus is false
        ///   2) - the wakeup. true if the bit is set; false otherwise.
        ///   3) - true if the read is good; it should be ignored if readStatus is false
        /// </returns>
        (byte theByte, bool wakeupBitSet, bool readStatus) ReadOneByte(bool isLongPoll);
    }
}
