namespace Aristocrat.Monaco.Hardware.Contracts.Gds
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    /// <summary>(Serializable) A device state.</summary>
    [Serializable]
    public class DeviceState : GdsSerializableMessage
    {
        /// <summary>Constructor</summary>
        public DeviceState() : base(GdsConstants.ReportId.DeviceState) { }

        /// <summary>Gets or sets the reserved bytes for future use 1.</summary>
        /// <value>The reserved bytes for future use 1.</value>
        [FieldOrder(0)]
        [FieldBitLength(6)]
        public byte Reserved1 { get; set; }

        /// <summary>
        /// Gets or sets a value indicating disabled.
        /// </summary>
        /// <value>True if disabled, false if not.</value>
        [FieldOrder(1)]
        [FieldBitLength(1)]
        public bool Disabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating enabled.
        /// </summary>
        /// <value>True if enabled, false if not.</value>
        [FieldOrder(2)]
        [FieldBitLength(1)]
        public bool Enabled { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant($"{GetType()} [Enabled={Enabled}, Disabled={Disabled}]");
        }
    }
}
