namespace Aristocrat.Monaco.Hardware.Contracts.Gds.Hopper
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    /// <summary>(Serializable) an hopper max out control command.</summary>
    [CLSCompliant(false)]
    [Serializable]

    public class HopperMaxOutControl: GdsSerializableMessage
    {
        /// <summary>Constructor</summary>
        public HopperMaxOutControl() : base(GdsConstants.ReportId.MaxCoinOutControll) { }

        /// <summary>Gets or sets the max hopper out count .</summary>
        /// <value>The max out count.</value>
        [FieldOrder(0)]
        public int Count { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant($"{GetType()} [Count={Count}]");
        }
    }
}
