namespace Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor
{
    using System;
    using static System.FormattableString;

    /// <summary>Definition of the Note Acceptor VoucherReturnedEvent class.</summary>
    /// <remarks>This event is posted when Note Acceptor has returned a voucher.</remarks>
    [Serializable]
    public class VoucherReturnedEvent : NoteAcceptorBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="VoucherReturnedEvent"/> class.
        /// </summary>
        /// <param name="barcode">A string representation of the barcode on the voucher.</param>
        public VoucherReturnedEvent(string barcode)
        {
            Barcode = barcode;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="VoucherReturnedEvent"/> class.
        /// </summary>
        /// <param name="noteAcceptorId">Identifier for the note acceptor.</param>
        /// <param name="barcode">A string representation of the barcode on the voucher.</param>
        public VoucherReturnedEvent(int noteAcceptorId, string barcode)
            : base(noteAcceptorId)
        {
            Barcode = barcode;
        }

        /// <summary>
        ///     Gets the human-readable barcode value.
        /// </summary>
        public string Barcode { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"{base.ToString()} [Barcode={Barcode}]");
        }
    }
}