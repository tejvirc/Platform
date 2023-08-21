namespace Aristocrat.Monaco.Hardware.Contracts.Gds.NoteAcceptor
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    /// <summary>(Serializable) a note validated.</summary>
    [Serializable]
    public class NoteValidated : GdsSerializableMessage, IEquatable<NoteValidated>, ITransactionSource
    {
        /// <summary>Constructor</summary>
        public NoteValidated() : base(GdsConstants.ReportId.NoteAcceptorNoteValidated) { }

        /// <summary>Gets or sets the identifier of the transaction.</summary>
        /// <value>The identifier of the transaction.</value>
        [FieldOrder(0)]
        public byte TransactionId { get; set; }

        /// <summary>Gets or sets the identifier of the note.</summary>
        /// <value>The identifier of the note.</value>
        [FieldOrder(1)]
        [FieldLength(1)]
        public int NoteId { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant($"{GetType()} [TransactionId={TransactionId}, NoteId={NoteId}]");
        }

        /// <inheritdoc/>
        public bool Equals(NoteValidated other)
        {
            if (other == null)
                return false;

            return NoteId == other.NoteId;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return Equals(obj as NoteValidated);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var hashCode = 11198884;
            hashCode *= 4998219 + NoteId.GetHashCode();
            return hashCode;
        }
    }
}