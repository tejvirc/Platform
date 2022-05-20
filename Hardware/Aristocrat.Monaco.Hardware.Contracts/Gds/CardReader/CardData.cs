namespace Aristocrat.Monaco.Hardware.Contracts.Gds.CardReader
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    /// <summary>(Serializable) a card data event report.</summary>
    [Serializable]
    public class CardData : GdsSerializableMessage, IDataReport
    {
        private const int CardReadPacketMax = 59;

        /// <summary>Constructor</summary>
        public CardData() : base(GdsConstants.ReportId.CardReaderCardData, CardReadPacketMax) { }

        /// <inheritdoc/>
        [FieldOrder(0)]
        [FieldLength(2)]
        [FieldEndianness(Endianness.Little)]
        public int Index { get; set; }

        /// <inheritdoc/>
        [FieldOrder(1)]
        [FieldLength(1)]
        public int Length { get; set; }

        /// <summary>Gets or sets the length.</summary>
        /// <value>The length.</value>
        [FieldOrder(2)]
        public byte Type { get; set; }

        /// <summary>Gets or sets the data.</summary>
        /// <value>The data.</value>
        [FieldOrder(3)]
        [FieldLength(nameof(Length))]
        [FieldEncoding("UTF-8")]
        public string Data { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant($"{GetType()} [Index={Index}, Length={Length}, Type={Type}, Data={Data}]");
        }
    }
}