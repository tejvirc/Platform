namespace Aristocrat.Monaco.Hardware.Contracts.Gds.Printer
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    /// <summary>(Serializable) a define template.</summary>
    [Serializable]
    public class DefineTemplate : GdsSerializableMessage, IDataReport
    {
        private const int TemplatePacketMax = 61;

        /// <summary>Constructor</summary>
        public DefineTemplate() : base(GdsConstants.ReportId.PrinterDefineTemplate, TemplatePacketMax) { }

        /// <inheritdoc/>
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