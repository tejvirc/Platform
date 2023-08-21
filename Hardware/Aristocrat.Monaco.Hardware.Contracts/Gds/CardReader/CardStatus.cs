namespace Aristocrat.Monaco.Hardware.Contracts.Gds.CardReader
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    /// <summary>(Serializable) a read card data.</summary>
    [Serializable]
    public class CardStatus : GdsSerializableMessage
    {
        /// <summary>Constructor</summary>
        public CardStatus() : base(GdsConstants.ReportId.CardReaderCardStatus) { }

        /// <summary>Gets or sets the reserved bytes for future use 1.</summary>
        /// <value>The reserved bytes for future use 1.</value>
        [FieldOrder(0)]
        [FieldBitLength(4)]
        public byte Reserved1 { get; set; }

        /// <summary>Gets or sets a value indicating whether a card is partially inserted.</summary>
        /// <value>True if partially inserted, false if not.</value>
        [FieldOrder(1)]
        [FieldBitLength(1)]
        public bool PartiallyInserted { get; set; }

        /// <summary>Gets or sets a value indicating whether a card is present.</summary>
        /// <value>True if card present, false if not.</value>
        [FieldOrder(2)]
        [FieldBitLength(1)]
        public bool CardPresent { get; set; }

        /// <summary>Gets or sets a value indicating whether a card is removed.</summary>
        /// <value>True if removed, false if not.</value>
        [FieldOrder(3)]
        [FieldBitLength(1)]
        public bool Removed { get; set; }

        /// <summary>Gets or sets a value indicating whether a card is inserted.</summary>
        /// <value>True if inserted, false if not.</value>
        [FieldOrder(4)]
        [FieldBitLength(1)]
        public bool Inserted { get; set; }

        /// <summary>Gets or sets the reserved bytes for future use 2.</summary>
        /// <value>The reserved bytes for future use 2.</value>
        [FieldOrder(5)]
        [FieldBitLength(4)]
        public byte Reserved2 { get; set; }

        /// <summary>Gets or sets a value indicating whether data is available on track 3.</summary>
        /// <value>True if data is available on track 3, false if not.</value>
        [FieldOrder(6)]
        [FieldBitLength(1)]
        public bool Track3 { get; set; }

        /// <summary>Gets or sets a value indicating whether data is available on track 2.</summary>
        /// <value>True if data is available on track 2, false if not.</value>
        [FieldOrder(7)]
        [FieldBitLength(1)]
        public bool Track2 { get; set; }

        /// <summary>Gets or sets a value indicating whether data is available on track 1.</summary>
        /// <value>True if data is available track 1, false if not.</value>
        [FieldOrder(8)]
        [FieldBitLength(1)]
        public bool Track1 { get; set; }

        /// <summary>Gets or sets a value indicating whether data is available on integrated circuit card.</summary>
        /// <value>True if data is available on integrated circuit card, false if not.</value>
        [FieldOrder(9)]
        [FieldBitLength(1)]
        public bool Icc { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant(
                $"{GetType()} [Inserted={Inserted}, Removed={Removed}, Present={CardPresent}, PartiallyInserted={PartiallyInserted}, Icc={Icc}, Track1={Track1}, Track2={Track2}, Track3={Track3}]");
        }
    }
}