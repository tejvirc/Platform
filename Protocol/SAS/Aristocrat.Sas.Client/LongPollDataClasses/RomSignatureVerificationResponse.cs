namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    using System.Collections.Generic;

    /// <inheritdoc />
    public class RomSignatureVerificationResponse : ISasMessage
    {
        /// <summary>
        ///     The response message for a ROM signature Long poll
        /// </summary>
        /// <param name="address">The SAS address to use for this message</param>
        /// <param name="signature">The signature to send back</param>
        public RomSignatureVerificationResponse(byte address, ushort signature)
        {
            var bytes = new List<byte> { address, (byte)LongPoll.RomSignatureVerification };
            bytes.AddRange(Utilities.ToBinary(signature, sizeof(ushort)));
            MessageData = bytes;
        }

        /// <inheritdoc />
        public IReadOnlyCollection<byte> MessageData { get; }
    }
}