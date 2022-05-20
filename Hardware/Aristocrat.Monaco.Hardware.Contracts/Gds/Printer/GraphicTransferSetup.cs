namespace Aristocrat.Monaco.Hardware.Contracts.Gds.Printer
{
    using System;
    using BinarySerialization;
    using Contracts.Printer;
    using static System.FormattableString;

    /// <summary>(Serializable) a graphic transfer setup command.</summary>
    [CLSCompliant(false)]
    [Serializable]
    public class GraphicTransferSetup : GdsSerializableMessage
    {
        /// <summary>Constructor</summary>
        public GraphicTransferSetup() : base(GdsConstants.ReportId.PrinterGraphicTransferSetup) { }

        /// <summary>Gets or sets the type of the graphic.</summary>
        /// <value>The type of the graphic.</value>
        [FieldOrder(0)]
        [FieldLength(1)]
        public GraphicFileType GraphicType { get; set; }

        /// <summary>Gets or sets the zero-based index of the graphic.</summary>
        /// <value>The graphic index.</value>
        [FieldOrder(1)]
        [FieldLength(1)]
        public byte GraphicIndex { get; set; }

        /// <summary>Gets or sets the size of the file.</summary>
        /// <value>The size of the file.</value>
        [FieldOrder(2)]
        [FieldLength(2)]
        [FieldEndianness(Endianness.Little)]
        public ushort FileSize { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant($"{GetType()} [GraphicType={GraphicType}] [GraphicIndex={GraphicIndex}] [FileSize={FileSize}]");
        }
    }
}