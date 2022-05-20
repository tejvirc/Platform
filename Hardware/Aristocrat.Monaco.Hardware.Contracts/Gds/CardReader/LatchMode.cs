namespace Aristocrat.Monaco.Hardware.Contracts.Gds.CardReader
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    /// <summary>(Serializable) a latch mode command.</summary>
    [Serializable]
    public class LatchMode : GdsSerializableMessage
    {
        /// <summary>Constructor</summary>
        public LatchMode() : base(GdsConstants.ReportId.CardReaderLatchMode) { }

        /// <summary>Gets or sets the reserved bytes for future use 1.</summary>
        /// <value>The reserved bytes for future use 1.</value>
        [FieldOrder(0)]
        [FieldBitLength(6)]
        public byte Reserved1 { get; set; }

        /// <summary>Gets or sets a value indicating whether the release.</summary>
        /// <value>True if release, false if not.</value>
        [FieldOrder(1)]
        [FieldBitLength(1)]
        public bool Release { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this Aristocrat.Monaco.Hardware.CardReader.LatchMode is locked.
        /// </summary>
        /// <value>True if lock, false if not.</value>
        [FieldOrder(2)]
        [FieldBitLength(1)]
        public bool Lock { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant($"{GetType()} [Release={Release}, Lock={Lock}]");
        }
    }
}