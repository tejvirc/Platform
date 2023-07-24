namespace Aristocrat.Monaco.Hardware.Contracts.PWM
{
    using Aristocrat.Monaco.Hardware.Contracts.Gds;
    using Aristocrat.Monaco.Hardware.Contracts.PWM;
    using BinarySerialization;
    using static System.FormattableString;

    /// <summary>
    /// 
    /// </summary>
    public class CoinInFaultStatus : GdsSerializableMessage
    {
        /// <summary>Constructor</summary>
        public CoinInFaultStatus() : base(GdsConstants.ReportId.CoinInFaultStatus) { }

        /// <summary>Gets or sets the identifier of the Coin In Fault.</summary>
        /// <value>The identifier of the transaction.</value>
        [FieldOrder(0)]
        public CoinFaultTypes FaultType { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant($"{GetType()} [CoinFaultTypes={FaultType}]");
        }
    }
}
