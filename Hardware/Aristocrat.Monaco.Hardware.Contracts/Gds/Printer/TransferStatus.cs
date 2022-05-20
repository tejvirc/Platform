namespace Aristocrat.Monaco.Hardware.Contracts.Gds.Printer
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    /// <summary>(Serializable) a transfer status.</summary>
    [Serializable]
    public class TransferStatus : GdsSerializableMessage
    {
        /// <summary>Constructor</summary>
        public TransferStatus() : base(GdsConstants.ReportId.PrinterTransferStatus) { }

        /// <summary>Gets or sets the reserved bytes for future use 1.</summary>
        /// <value>The reserved bytes for future use 1.</value>
        [FieldOrder(0)]
        [FieldBitLength(4)]
        public byte Reserved1 { get; set; }

        /// <summary>Gets or sets a value indicating whether there is a graphic code.</summary>
        /// <value>True if a graphic code is specified, false if not.</value>
        [FieldOrder(1)]
        [FieldBitLength(1)]
        public bool GraphicCode { get; set; }

        /// <summary>Gets or sets a value indicating whether there is a region code.</summary>
        /// <value>True if a region code is specified, false if not.</value>
        [FieldOrder(2)]
        [FieldBitLength(1)]
        public bool RegionCode { get; set; }

        /// <summary>Gets or sets a value indicating whether there is a template code.</summary>
        /// <value>True if a template code is specified, false if not.</value>
        [FieldOrder(3)]
        [FieldBitLength(1)]
        public bool TemplateCode { get; set; }

        /// <summary>Gets or sets a value indicating whether there is a print code.</summary>
        /// <value>True if a print code is specified, false if not.</value>
        [FieldOrder(4)]
        [FieldBitLength(1)]
        public bool PrintCode { get; set; }

        /// <summary>Gets or sets the status code.</summary>
        /// <value>The status code.</value>
        [FieldOrder(5)]
        public byte StatusCode { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant($"{GetType()} [PrintCode={PrintCode}, TemplateCode={TemplateCode}, RegionCode={RegionCode}, GraphicCode={GraphicCode}, StatusCode={StatusCode}]");
        }
    }
}