namespace Aristocrat.Monaco.Hardware.Contracts.Gds.NoteAcceptor
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    /// <summary>(Serializable) a ticket validated utf.</summary>
    [Serializable]
    public class UtfTicketValidated : GdsSerializableMessage, IDataReport, IEquatable<UtfTicketValidated>, ITransactionSource
    {
        private const int UtfTicketPacketMax = 60;

        /// <summary>Constructor</summary>
        public UtfTicketValidated() : base(GdsConstants.ReportId.NoteAcceptorUtfTicketValidated, UtfTicketPacketMax) { }

        /// <summary>Gets or sets the identifier of the transaction.</summary>
        /// <value>The identifier of the transaction.</value>
        [FieldOrder(0)]
        public byte TransactionId { get; set; }

        /// <inheritdoc/>
        [FieldOrder(1)]
        [FieldLength(1)]
        public int Index { get; set; }

        /// <inheritdoc />
        [FieldOrder(2)]
        [FieldLength(1)]
        public int Length { get; set; }

        /// <inheritdoc/>
        [FieldOrder(3)]
        [FieldLength(nameof(Length))]
        [FieldEncoding("UTF-16")]
        [FieldEndianness(Endianness.Little)]
        public string Data { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant($"{GetType()} [TransactionId={TransactionId}, Index={Index}, Length={Length}, Data={Data}]");
        }

        /// <inheritdoc/>
        public bool Equals(UtfTicketValidated other)
        {
            if (other == null)
                return false;

            return Length == other.Length &&
                   Data == other.Data;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return Equals(obj as UtfTicketValidated);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var hashCode = 89992543;
            hashCode *= 77743 + Length.GetHashCode();
            hashCode *= 77743 + Data.GetHashCode();
            return hashCode;
        }
    }
}