namespace Aristocrat.Monaco.Hardware.Contracts.Gds.CoinAcceptor
{
    using Contracts.Gds;
    using BinarySerialization;
    using static System.FormattableString;

    /// <summary>(Serializable) a reject mode command.</summary>
    public class RejectMode : GdsSerializableMessage
    {
        /// <summary>Constructor</summary>
        public RejectMode() : base(GdsConstants.ReportId.CoinValidatorRejectState) { }

        /// <summary>Gets or sets the reserved bytes for future use 1.</summary>
        /// <value>The reserved bytes for future use 1.</value>
        [FieldOrder(0)]
        [FieldBitLength(1)]
        public bool RejectOnOff { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant($"{GetType()} [RejectOnOff={RejectOnOff}]");
        }
    }
}
