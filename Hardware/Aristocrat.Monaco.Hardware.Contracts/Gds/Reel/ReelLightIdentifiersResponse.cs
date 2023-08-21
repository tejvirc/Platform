namespace Aristocrat.Monaco.Hardware.Contracts.Gds.Reel
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    /// <summary>
    ///     The response for a get reel light identifier command
    /// </summary>
    [Serializable]
    public class ReelLightIdentifiersResponse : GdsSerializableMessage, ITransactionSource
    {
        /// <summary>
        ///     Creates an instance of <see cref="ReelLightIdentifiersResponse"/>
        /// </summary>
        public ReelLightIdentifiersResponse()
            : base(GdsConstants.ReportId.ReelControllerLightIdentifiersResponse)
        {
        }

        /// <summary> Gets or sets the transaction Id for this event </summary>
        [FieldOrder(0)]
        public byte TransactionId { get; set; }

        /// <summary>Gets or sets the starting identifier value</summary>
        [FieldOrder(1)]
        [FieldBitLength(8)]
        public int StartId { get; set; }

        /// <summary>Gets or sets the ending identifier value.</summary>
        /// <value>The reserved 1.</value>
        [FieldOrder(2)]
        [FieldBitLength(8)]
        public int EndId { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant(
                $"{GetType()} [StartId={StartId},EndId={EndId}]");
        }
    }
}