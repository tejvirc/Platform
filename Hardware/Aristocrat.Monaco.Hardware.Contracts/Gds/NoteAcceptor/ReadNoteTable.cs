namespace Aristocrat.Monaco.Hardware.Contracts.Gds.NoteAcceptor
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    /// <summary>(Serializable) read a note table.</summary>
    [CLSCompliant(false)]
    [Serializable]
    public class ReadNoteTable : GdsSerializableMessage
    {
        /// <summary>Constructor</summary>
        public ReadNoteTable() : base(GdsConstants.ReportId.NoteAcceptorReadNoteTable) { }

        /// <summary>Gets or sets the identifier of the note.</summary>
        /// <value>Note ID in Note Table.</value>
        [FieldOrder(0)]
        [FieldLength(1)]
        public int NoteId { get; set; }

        /// <summary>Gets or sets the currency.</summary>
        /// <value>The currency of the Note.</value>
        [FieldOrder(1)]
        [FieldLength(3)]
        [FieldEncoding("ASCII")]
        public string Currency { get; set; }

        /// <summary>Gets or sets the value.</summary>
        /// <value>The value.</value>
        [FieldOrder(2)]
        [FieldLength(2)]
        [FieldEndianness(Endianness.Little)]
        public ushort Value { get; set; }

        /// <summary>Gets or sets a value indicating sign of the scalar.</summary>
        /// <value>Positive(+) if True, Negative(-) otherwise.</value>
        [FieldOrder(3)]
        [FieldBitLength(1)]
        public bool Sign { get; set; }

        /// <summary>Gets or sets the scalar.</summary>
        /// <value>The scalar.</value>
        [FieldOrder(4)]
        [FieldBitLength(7)]
        public byte Scalar { get; set; }

        /// <summary>Gets or sets the version.</summary>
        /// <value>The version of the Note denomination.</value>
        [FieldOrder(5)]
        public byte Version { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant($"{GetType()} [NoteId={NoteId}, Currency={Currency}, Value={Value}, Sign={Sign}, Scalar={Scalar}, Version={Version}]");
        }
    }
}