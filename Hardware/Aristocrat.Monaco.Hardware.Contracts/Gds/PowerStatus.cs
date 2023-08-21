namespace Aristocrat.Monaco.Hardware.Contracts.Gds
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    /// <summary>(Serializable) a power status.</summary>
    [Serializable]
    public class PowerStatus : GdsSerializableMessage
    {
        /// <summary>Constructor</summary>
        public PowerStatus() : base(GdsConstants.ReportId.PowerStatus) { }

        /// <summary>Gets or sets the reserved bytes for future use 1.</summary>
        /// <value>The reserved bytes for future use 1.</value>
        [FieldOrder(0)]
        [FieldBitLength(5)]
        public byte Reserved1 { get; set; }

        /// <summary>Gets or sets a value indicating whether the battery failed.</summary>
        /// <value>True if battery failed, false if not.</value>
        [FieldOrder(1)]
        [FieldBitLength(1)]
        public bool BatteryFailed { get; set; }

        /// <summary>Gets or sets a value indicating whether the need reset.</summary>
        /// <value>True if need reset, false if not.</value>
        [FieldOrder(2)]
        [FieldBitLength(1)]
        public bool RequiresReset { get; set; }

        /// <summary>Gets or sets a value indicating whether the external power.</summary>
        /// <value>True if external power, false if not.</value>
        [FieldOrder(3)]
        [FieldBitLength(1)]
        public bool ExternalPower { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant($"{GetType()} [RequiresReset={RequiresReset}, ExternalPower={ExternalPower}, BatteryFailed={BatteryFailed}]");
        }
    }
}
