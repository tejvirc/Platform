namespace Aristocrat.Monaco.Hardware.Contracts.Gds.CardReader
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    /// <summary>(Serializable) a read card data command.</summary>
    [Serializable]
    public class ReadCardData : GdsSerializableMessage
    {
        /// <summary>Constructor</summary>
        public ReadCardData() : base(GdsConstants.ReportId.CardReaderReadCardData) { }

        /// <summary>Gets or sets the reserved bytes for future use 1.</summary>
        /// <value>The reserved bytes for future use 1.</value>
        [FieldOrder(0)]
        [FieldBitLength(5)]
        public byte Reserved1 { get; set; }

        /// <summary>Gets or sets a value indicating whether to read track 3.</summary>
        /// <value>True if track 3 should be read, false if not.</value>
        [FieldOrder(1)]
        [FieldBitLength(1)]
        public bool Track3 { get; set; }

        /// <summary>Gets or sets a value indicating whether to read track 2.</summary>
        /// <value>True if track 2 should be read, false if not.</value>
        [FieldOrder(2)]
        [FieldBitLength(1)]
        public bool Track2 { get; set; }

        /// <summary>Gets or sets a value indicating whether to read track 1.</summary>
        /// <value>True if track 1 should be read, false if not.</value>
        [FieldOrder(3)]
        [FieldBitLength(1)]
        public bool Track1 { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant($"{GetType()} [Track1={Track1}, Track2={Track2}, Track3={Track3}]");
        }
    }
}