namespace Aristocrat.Monaco.Hardware.Contracts.Gds.CardReader
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    /// <summary>(Serializable) a count status.</summary>
    [Serializable]
    public class CountStatus : GdsSerializableMessage
    {
        /// <summary>Constructor</summary>
        public CountStatus() : base(GdsConstants.ReportId.CardReaderCountStatus) { }

        /// <summary>Gets or sets the number of times a magnetic card has been inserted.</summary>
        /// <value>The number of times a magnetic card has been inserted.</value>
        [FieldOrder(0)]
        [FieldLength(3)]
        public int CardInsertedCount { get; set; }

        /// <summary>Gets or sets the number of track 1 errors.</summary>
        /// <value>The number of track 1 errors.</value>
        [FieldOrder(1)]
        [FieldLength(3)]
        public int Track1ErrorCount { get; set; }

        /// <summary>Gets or sets the number of track 2 errors.</summary>
        /// <value>The number of track 2 errors.</value>
        [FieldOrder(2)]
        [FieldLength(3)]
        public int Track2ErrorCount { get; set; }

        /// <summary>Gets or sets the number of track 3 errors.</summary>
        /// <value>The number of track 3 errors.</value>
        [FieldOrder(3)]
        [FieldLength(3)]
        public int Track3ErrorCount { get; set; }

        /// <summary>Gets or sets the number of times an ICC card has been inserted.</summary>
        /// <value>The number of times an ICC card has been inserted.</value>
        [FieldOrder(4)]
        [FieldLength(3)]
        public int IccInsertedCount { get; set; }

        /// <summary>Gets or sets the number of ICC errors.</summary>
        /// <value>The number of ICC errors.</value>
        [FieldOrder(5)]
        [FieldLength(3)]
        public int IccErrorCount { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant(
                $"{GetType()} [CardInsertedCount={CardInsertedCount}, Track1ErrorCount={Track1ErrorCount}, Track2ErrorCount={Track2ErrorCount}, Track3ErrorCount={Track3ErrorCount}, IccInsertedCount={IccInsertedCount}, IccErrorCount={IccErrorCount}]");
        }

    }
}