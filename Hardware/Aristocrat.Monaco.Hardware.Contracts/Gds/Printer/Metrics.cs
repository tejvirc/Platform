namespace Aristocrat.Monaco.Hardware.Contracts.Gds.Printer
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    /// <summary>(Serializable) a metrics.</summary>
    [Serializable]
    public class Metrics : GdsSerializableMessage, IDataReport
    {
        private const int MetricsPacketMax = 61;

        /// <summary>Constructor</summary>
        public Metrics() : base(GdsConstants.ReportId.PrinterMetrics, MetricsPacketMax) { }

        /// <summary>Gets or sets the zero-based index of this object.</summary>
        /// <value>The index.</value>
        [FieldOrder(0)]
        [FieldLength(1)]
        public int Index { get; set; }

        /// <inheritdoc/>
        [FieldOrder(1)]
        [FieldLength(1)]
        public int Length { get; set; }

        /// <summary>Gets or sets the data.</summary>
        /// <value>The data.</value>
        [FieldOrder(2)]
        [FieldLength(nameof(Length))]
        [FieldEncoding("ASCII")]
        public string Data { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant($"{GetType()} [Index={Index}, Length={Length}, Data={Data}]");
        }
    }
}