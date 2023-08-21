namespace Aristocrat.Monaco.Hardware.Contracts.Gds.NoteAcceptor
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    /// <summary>(Serializable) a ticket validated.</summary>
    [Serializable]
    public class TicketValidated : GdsSerializableMessage, IEquatable<TicketValidated>, ITransactionSource
    {
        /// <summary>Constructor</summary>
        public TicketValidated() : base(GdsConstants.ReportId.NoteAcceptorTicketValidated) { }

        /// <summary>Gets or sets the identifier of the transaction.</summary>
        /// <value>The identifier of the transaction.</value>
        [FieldOrder(0)]
        public byte TransactionId { get; set; }

        /// <summary>Gets or sets the length.</summary>
        /// <value>The length.</value>
        [FieldOrder(1)]
        public byte Length { get; set; }

        /// <summary>Gets or sets the validation code.</summary>
        /// <value>The validation code.</value>
        [FieldOrder(2)]
        [FieldLength(nameof(Length))]
        public string Code { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant($"{GetType()} [TransactionId={TransactionId}, Length={Length}, Code={Code}]");
        }

        /// <inheritdoc/>
        public bool Equals(TicketValidated other)
        {
            if (other == null)
                return false;

            return Length == other.Length &&
                   Code == other.Code;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return Equals(obj as TicketValidated);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var hashCode = 2000579;
            hashCode *= 8005432 + Length.GetHashCode();
            hashCode *= 8005432 + Code.GetHashCode();
            return hashCode;
        }
    }
}