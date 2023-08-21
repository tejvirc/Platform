namespace Aristocrat.Monaco.Hardware.Contracts.Gds.Reel
{
    using System;
    using BinarySerialization;

    /// <summary>
    /// Response for Tilt Reels command
    /// </summary>
    [Serializable]
    public class TiltReelsResponse : GdsSerializableMessage, ITransactionSource
    {
        /// <summary>
        ///     Creates an instance of <see cref="TiltReelsResponse"/>
        /// </summary>
        public TiltReelsResponse()
            : base(GdsConstants.ReportId.ReelControllerTiltReelsResponse)
        {
        }

        /// <summary> Gets or sets the transaction Id for this event </summary>
        [FieldOrder(0)]
        public byte TransactionId { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return FormattableString.Invariant($"TiltReelsResponse [ReportId={ReportId}, TransactionId={TransactionId}]");
        }
    }
}
