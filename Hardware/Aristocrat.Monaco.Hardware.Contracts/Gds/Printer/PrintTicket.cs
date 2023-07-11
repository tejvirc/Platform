namespace Aristocrat.Monaco.Hardware.Contracts.Gds.Printer
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    /// <summary>(Serializable) a print ticket.</summary>
    [Serializable]
    public class PrintTicket : GdsSerializableMessage, IDataReport
    {
        private const int PrintPacketMax = 61;

        /// <summary>Constructor</summary>
        public PrintTicket() : base(GdsConstants.ReportId.PrinterPrintTicket, PrintPacketMax) { }

        /// <summary>Gets or sets the zero-based index of this multi-part message.</summary>
        /// <value>The index.</value>
        [FieldOrder(0)]
        [FieldLength(1)]
        public int Index { get; set; }

        /// <summary>Gets or sets the length.</summary>
        /// <value>The length.</value>
        [FieldOrder(1)]
        [FieldLength(1)]
        public int Length { get; set; }

        /// <summary>Gets or sets the data.</summary>
        /// <value>The data.</value>
        [FieldOrder(2)]
        [FieldLength(nameof(Length))]
        [FieldEncoding("iso-8859-1")]
        public string Data { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant($"{GetType()} [Index={Index}, Length={Length}, Data={Data}]");
        }
    }
}