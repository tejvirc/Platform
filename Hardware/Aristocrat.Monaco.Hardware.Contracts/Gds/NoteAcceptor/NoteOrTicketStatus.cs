namespace Aristocrat.Monaco.Hardware.Contracts.Gds.NoteAcceptor
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    /// <summary>(Serializable) a ticket status.</summary>
    [Serializable]
    public class NoteOrTicketStatus : GdsSerializableMessage, IEquatable<NoteOrTicketStatus>, ITransactionSource
    {
        /// <summary>Constructor</summary>
        public NoteOrTicketStatus() : base(GdsConstants.ReportId.NoteAcceptorNoteOrTicketStatus) { }

        /// <summary>Gets or sets the identifier of the transaction.</summary>
        /// <value>The identifier of the transaction.</value>
        [FieldOrder(0)]
        public byte TransactionId { get; set; }

        /// <summary>Gets or sets a value indicating whether the jam.</summary>
        /// <value>True if jam, false if not.</value>
        [FieldOrder(1)]
        [FieldBitLength(1)]
        public bool Jam { get; set; }

        /// <summary>Gets or sets a value indicating whether the cheat.</summary>
        /// <value>True if cheat, false if not.</value>
        [FieldOrder(2)]
        [FieldBitLength(1)]
        public bool Cheat { get; set; }

        /// <summary>Gets or sets the reserved 1.</summary>
        /// <value>The reserved 1.</value>
        [FieldOrder(3)]
        [FieldBitLength(1)]
        public byte Reserved1 { get; set; }

        /// <summary>Gets or sets a value indicating whether the path clear.</summary>
        /// <value>True if path clear, false if not.</value>
        [FieldOrder(4)]
        [FieldBitLength(1)]
        public bool PathClear { get; set; }

        /// <summary>Gets or sets a value indicating whether the removed.</summary>
        /// <value>True if removed, false if not.</value>
        [FieldOrder(5)]
        [FieldBitLength(1)]
        public bool Removed { get; set; }

        /// <summary>Gets or sets a value indicating whether the rejected.</summary>
        /// <value>True if rejected, false if not.</value>
        [FieldOrder(6)]
        [FieldBitLength(1)]
        public bool Rejected { get; set; }

        /// <summary>Gets or sets a value indicating whether the returned.</summary>
        /// <value>True if returned, false if not.</value>
        [FieldOrder(7)]
        [FieldBitLength(1)]
        public bool Returned { get; set; }

        /// <summary>Gets or sets a value indicating whether the accepted.</summary>
        /// <value>True if accepted, false if not.</value>
        [FieldOrder(8)]
        [FieldBitLength(1)]
        public bool Accepted { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant($"{GetType()} [TransactionId={TransactionId}, Jam={Jam}, Cheat={Cheat}, PathClear={PathClear}, Removed={Removed}, Rejected={Rejected}, Returned={Returned}, Accepted={Accepted}]");
        }

        /// <inheritdoc/>
        public bool Equals(NoteOrTicketStatus other)
        {
            if (other == null)
                return false;

            return Jam == other.Jam &&
                   Cheat == other.Cheat &&
                   PathClear == other.PathClear &&
                   Removed == other.Removed &&
                   Rejected == other.Rejected &&
                   Returned == other.Returned &&
                   Accepted == other.Accepted;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return Equals(obj as NoteOrTicketStatus);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var hashCode = 57762333;
            hashCode *= 539 + Jam.GetHashCode();
            hashCode *= 539 + Cheat.GetHashCode();
            hashCode *= 539 + PathClear.GetHashCode();
            hashCode *= 539 + Removed.GetHashCode();
            hashCode *= 539 + Rejected.GetHashCode();
            hashCode *= 539 + Returned.GetHashCode();
            hashCode *= 539 + Accepted.GetHashCode();
            return hashCode;
        }
    }
}