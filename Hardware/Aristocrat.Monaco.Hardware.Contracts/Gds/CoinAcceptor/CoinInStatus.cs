namespace Aristocrat.Monaco.Hardware.Contracts.Gds.CoinAcceptor
{
    using PWM;
    using BinarySerialization;
    using static System.FormattableString;

    /// <summary>
    ///     Coin In Status
    /// </summary>
    public class CoinInStatus : GdsSerializableMessage
    {
        /// <summary>Constructor</summary>
        public CoinInStatus() : base(GdsConstants.ReportId.CoinInStatus) { }

        /// <summary>Gets or sets the identifier of the Coin In.</summary>
        /// <value>The identifier of the transaction.</value>
        [FieldOrder(0)]
        public CoinEventType EventType { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant($"{GetType()} [CoinEventType={EventType}]");
        }
    }
}
