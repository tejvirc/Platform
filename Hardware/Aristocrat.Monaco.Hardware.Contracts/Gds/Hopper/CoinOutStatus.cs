namespace Aristocrat.Monaco.Hardware.Contracts.Gds.Hopper
{
    using BinarySerialization;
    using static System.FormattableString;

    /// <summary>
    ///     Coin Out Status
    /// </summary>
    public class CoinOutStatus : GdsSerializableMessage
    {
        /// <summary>Constructor</summary>
        public CoinOutStatus() : base(GdsConstants.ReportId.CoinOutStatus) { }

        /// <summary>Gets or sets the field of Legal.</summary>
        /// <value>The field of Legal.</value>
        [FieldOrder(0)]
        [FieldBitLength(1)]
        public bool Legal { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant($"{GetType()} [Legal={Legal}]");
        }
    }
}
