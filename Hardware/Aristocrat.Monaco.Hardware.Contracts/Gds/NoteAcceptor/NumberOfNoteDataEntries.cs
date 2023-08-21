namespace Aristocrat.Monaco.Hardware.Contracts.Gds.NoteAcceptor
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    /// <summary>(Serializable) a number of note data entries.</summary>
    [Serializable]
    public class NumberOfNoteDataEntries : GdsSerializableMessage
    {
        /// <summary>Constructor</summary>
        public NumberOfNoteDataEntries() : base(GdsConstants.ReportId.NoteAcceptorNumberOfNoteDataEntries) { }

        /// <summary>Gets or sets the number of entries.</summary>
        /// <value>The number of entries.</value>
        [FieldOrder(0)]
        public byte Number { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant($"{GetType()} [Number={Number}]");
        }
    }
}