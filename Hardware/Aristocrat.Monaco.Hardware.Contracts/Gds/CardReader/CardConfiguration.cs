namespace Aristocrat.Monaco.Hardware.Contracts.Gds.CardReader
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    /// <summary>(Serializable) a card configuration.</summary>
    [Serializable]
    public class CardConfiguration : GdsSerializableMessage
    {
        /// <summary>Constructor</summary>
        public CardConfiguration() : base(GdsConstants.ReportId.CardReaderConfigData) { }

        /// <summary>Gets or sets the reserved bytes for future use 1.</summary>
        /// <value>The reserved bytes for future use 1.</value>
        [FieldOrder(0)]
        [FieldBitLength(2)]
        public byte Reserved1 { get; set; }

        /// <summary>Gets or sets a value indicating whether extended light is supported.</summary>
        /// <value>True if extended light support, false if not.</value>
        [FieldOrder(1)]
        [FieldBitLength(1)]
        public bool ExtendedLightSupport { get; set; }

        /// <summary>Gets or sets a value indicating whether the latch mechanism is supported.</summary>
        /// <value>True if latch, false if not.</value>
        [FieldOrder(2)]
        [FieldBitLength(1)]
        public bool Latch { get; set; }

        /// <summary>Gets or sets a value indicating whether track 3 is supported.</summary>
        /// <value>True if track 3 should be read, false if not.</value>
        [FieldOrder(3)]
        [FieldBitLength(1)]
        public bool Track3 { get; set; }

        /// <summary>Gets or sets a value indicating whether track 2 is supported.</summary>
        /// <value>True if track 2 should be read, false if not.</value>
        [FieldOrder(4)]
        [FieldBitLength(1)]
        public bool Track2 { get; set; }

        /// <summary>Gets or sets a value indicating whether track 1 is supported.</summary>
        /// <value>True if track 1 should be read, false if not.</value>
        [FieldOrder(5)]
        [FieldBitLength(1)]
        public bool Track1 { get; set; }

        /// <summary>Gets or sets a value indicating whether ICC is supported.</summary>
        /// <value>True if ICC is supported, false if not.</value>
        [FieldOrder(6)]
        [FieldBitLength(1)]
        public bool Icc { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant(
                $"{GetType()} [Icc={Icc}, Track1={Track1}, Track2={Track2}, Track3={Track3}, Latch={Latch}, ExtendedLightSupport={ExtendedLightSupport}]");
        }
    }
}