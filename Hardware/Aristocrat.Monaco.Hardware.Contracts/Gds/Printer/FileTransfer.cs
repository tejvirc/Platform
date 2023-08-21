namespace Aristocrat.Monaco.Hardware.Contracts.Gds.Printer
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    /// <summary>(Serializable) a file transfer command.</summary>
    [Serializable]
    public class FileTransfer : GdsSerializableMessage, IDataReport
    {
        private const int FileTransferMax = 60;

        /// <summary>Constructor</summary>
        public FileTransfer() : base(GdsConstants.ReportId.PrinterFileTransfer, FileTransferMax) { }

        /// <inheritdoc/>
        [FieldOrder(0)]
        [FieldLength(2)]
        public int Index { get; set; }

        /// <inheritdoc/>
        [FieldOrder(1)]
        [FieldLength(1)]
        public int Length { get; set; }

        /// <summary>Gets or sets the data.</summary>
        /// <value>The data.</value>
        [FieldOrder(2)]
        [FieldLength(nameof(Length))]
        [FieldEncoding("UTF-8")]
        public string Data { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant($"{GetType()} [Index={Index}, Length={Length}, Data={Data}]");
        }
    }
}