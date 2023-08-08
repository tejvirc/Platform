namespace Aristocrat.Monaco.Hardware.Contracts.Gds.Hopper
{
    using BinarySerialization;
    using Contracts.Hopper;
    using static System.FormattableString;

    /// <summary>
    ///     Hopper Fault Status
    /// </summary>
    public class HopperFaultStatus : GdsSerializableMessage
    {
        /// <summary>Constructor</summary>
        public HopperFaultStatus() : base(GdsConstants.ReportId.HopperFaultStatus) { }

        /// <summary>Gets or sets the identifier of the Hopper Fault.</summary>
        /// <value>Hopper fault.</value>
        [FieldOrder(0)]
        public HopperFaultTypes FaultType { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant($"{GetType()} [HopperFaultTypes={FaultType}]");
        }
    }
}
