namespace Aristocrat.Monaco.Hardware.Contracts.Gds.Hopper
{
    using BinarySerialization;
    using static System.FormattableString;
    /// <summary>
    ///     Hopper bowl Status
    /// </summary>
    public class HopperBowlStatus : GdsSerializableMessage
    {
        /// <summary>Constructor</summary>
        public HopperBowlStatus() : base(GdsConstants.ReportId.HopperBowlStatus) { }

        /// <summary>Gets or sets the status of Hopper bowl.</summary>
        /// <value>true if full otherwise false.</value>
        [FieldOrder(0)]
        [FieldBitLength(1)]
        public bool IsFull { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant($"{GetType()} [IsFull={IsFull}]");
        }
    }
}
