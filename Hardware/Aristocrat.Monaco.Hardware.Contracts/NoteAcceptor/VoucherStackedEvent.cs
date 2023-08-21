namespace Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor
{
    using System;
    using static System.FormattableString;

    /// <summary>Definition of the Note Acceptor VoucherStackedEvent class.</summary>
    /// <remarks>This event is posted when Note Acceptor has stacked a document.</remarks>
    [Serializable]
    public class VoucherStackedEvent : NoteAcceptorBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="VoucherStackedEvent" /> class.
        /// </summary>
        /// <param name="barcode">The barcode.</param>
        public VoucherStackedEvent(string barcode)
        {
            Barcode = barcode;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="VoucherStackedEvent" /> class.
        /// </summary>
        public VoucherStackedEvent(int noteAcceptorId, string barcode)
            : base(noteAcceptorId)
        {
            Barcode = barcode;
        }

        /// <summary>Gets the barcode.</summary>
        /// <value>The barcode.</value>
        public string Barcode { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"{base.ToString()} [Barcode={Barcode}]");
        }
    }
}
