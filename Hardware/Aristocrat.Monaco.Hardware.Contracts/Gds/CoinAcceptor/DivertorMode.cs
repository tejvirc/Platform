namespace Aristocrat.Monaco.Hardware.Contracts.Gds.CoinAcceptor
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    /// <summary>(Serializable) a divertor mode command.</summary>
    [Serializable]
    public class DivertorMode : GdsSerializableMessage
    {
        /// <summary>Constructor</summary>
        public DivertorMode() : base(GdsConstants.ReportId.CoinValidatorDivertorState) { }

        /// <summary>Gets or sets the reserved bytes for future use 1.</summary>
        /// <value>The reserved bytes for future use 1.</value>
        [FieldOrder(0)]
        [FieldBitLength(1)]
        public bool DivertorOnOff { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant($"{GetType()} [DivertorOnOff={DivertorOnOff}]");
        }
    }
}