namespace Aristocrat.Monaco.Hardware.Contracts.Gds.NoteAcceptor
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    /// <summary>(Serializable) a stacker status.</summary>
    [Serializable]
    public class StackerStatus : GdsSerializableMessage, IEquatable<StackerStatus>, ITransactionSource
    {
        /// <summary>Constructor</summary>
        public StackerStatus() : base(GdsConstants.ReportId.NoteAcceptorStackerStatus) { }

        /// <summary>Gets or sets the identifier of the transaction.</summary>
        /// <value>The identifier of the transaction.</value>
        [FieldOrder(0)]
        public byte TransactionId { get; set; }

        /// <summary>Gets or sets a value indicating whether the stacker is faulted.</summary>
        /// <value>True if the stacker is faulted, false if not.</value>
        [FieldOrder(1)]
        [FieldBitLength(1)]
        public bool Fault { get; set; }

        /// <summary>Gets or sets the reserved 1.</summary>
        /// <value>The reserved 1.</value>
        [FieldOrder(2)]
        [FieldBitLength(4)]
        public byte Reserved1 { get; set; }

        /// <summary>Gets or sets a value indicating whether the stacker is jammed.</summary>
        /// <value>True if the stacker is jammed, false if not.</value>
        [FieldOrder(3)]
        [FieldBitLength(1)]
        public bool Jam { get; set; }

        /// <summary>Gets or sets a value indicating whether the stacker is full.</summary>
        /// <value>True if the stacker is full false if not.</value>
        [FieldOrder(4)]
        [FieldBitLength(1)]
        public bool Full { get; set; }

        /// <summary>Gets or sets a value indicating whether the stacker is disconnected.</summary>
        /// <value>True if the stacker is disconnected, false if not.</value>
        [FieldOrder(5)]
        [FieldBitLength(1)]
        public bool Disconnect { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant($"{GetType()} [TransactionId={TransactionId}, Fault={Fault}, Jam={Jam}, Full={Full}, Disconnect={Disconnect}]");
        }

        /// <inheritdoc/>
        public bool Equals(StackerStatus other)
        {
            if (other == null)
                return false;

            return Fault == other.Fault &&
                   Jam == other.Jam &&
                   Full == other.Full &&
                   Disconnect == other.Disconnect;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return Equals(obj as StackerStatus);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var hashCode = 188000745;
            hashCode *= 19321 + Fault.GetHashCode();
            hashCode *= 19321 + Jam.GetHashCode();
            hashCode *= 19321 + Full.GetHashCode();
            hashCode *= 19321 + Disconnect.GetHashCode();
            return hashCode;
        }
    }
}