namespace Aristocrat.Monaco.Hardware.Contracts.Gds.Reel
{
    using System;
    using BinarySerialization;

    /// <summary>
    ///     Response for reel light updates
    /// </summary>
    [Serializable]
    public class ReelLightResponse : GdsSerializableMessage, ITransactionSource
    {
        /// <summary>
        ///     Creates an instance of <see cref="ReelLightResponse"/>
        /// </summary>
        public ReelLightResponse()
            : base(GdsConstants.ReportId.ReelControllerLightResponse)
        {
        }

        /// <inheritdoc />
        [FieldOrder(0)]
        public byte TransactionId { get; set; }

        /// <summary>Gets or sets the reserved bytes for future use 1.</summary>
        /// <value>The reserved bytes for future use 1.</value>
        [FieldOrder(1)]
        [FieldBitLength(7)]
        public byte Reserved1 { get; set; }

        /// <summary>
        ///     Gets or sets whether or not the lights were updated
        /// </summary>
        [FieldOrder(2)]
        [FieldBitLength(1)]
        public bool LightsUpdated { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return FormattableString.Invariant($"ReelLightResponse [ReportId={ReportId}, LightsUpdated={LightsUpdated}, TransactionId={TransactionId}]");
        }
    }
}