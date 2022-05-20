namespace Aristocrat.Monaco.Sas.Exceptions
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Sas.Client;
    using Gaming.Contracts;

    /// <summary>
    ///     A card held/not held exception builder. Note that this exception should only
    ///     be sent if real time reporting has been enabled for the host.
    /// </summary>
    [Serializable]
    public class CardHeldExceptionBuilder : List<byte>, ISasExceptionCollection
    {
        private const byte Held = 0x80;

        /// <summary>
        ///     Creates a card held exception
        /// </summary>
        /// <param name="held">'Yes' if the card was held</param>
        /// <param name="cardNumber">The number of the card in the hand. 0=left most card, 4=right most card</param>
        public CardHeldExceptionBuilder(HoldStatus held, int cardNumber)
        {
            Add((byte)ExceptionCode);

            // convert the held/not held information to numbers that exception 8B wants
            // see Table 12.5.7 on page 12-6 of the SAS 6.03 Spec.
            Add((byte)(cardNumber | (held == HoldStatus.Held ? Held : 0)));
        }

        /// <inheritdoc />
        public GeneralExceptionCode ExceptionCode => GeneralExceptionCode.CardHeldOrNotHeld;
    }
}