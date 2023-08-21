namespace Aristocrat.Monaco.Hardware.Contracts.Gds.CardReader
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    /// <summary>(Serializable) a transfer to icc.</summary>
    [Serializable]
    public class TransferToIcc : GdsSerializableMessage, IDataReport
    {
        /// <summary>Constructor</summary>
        public TransferToIcc() : base(GdsConstants.ReportId.CardReaderTransferToIcc) { }

        /// <inheritdoc/>
        [FieldOrder(0)]
        [FieldLength(2)]
        [FieldEndianness(Endianness.Little)]
        public int Index { get; set; }

        /// <inheritdoc/>
        [FieldOrder(1)]
        [FieldLength(1)]
        public int Length { get; set; }

        /// <summary>Gets or sets the data.</summary>
        /// <value>The data.</value>
        [FieldOrder(3)]
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