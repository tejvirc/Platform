namespace Aristocrat.Monaco.Hardware.Contracts.Gds.Hopper
{
    using System;
    using BinarySerialization;
    using static System.FormattableString;

    /// <summary>(Serializable) an hopper motor control command.</summary>
    [CLSCompliant(false)]
    [Serializable]
    public class HopperMotorControl : GdsSerializableMessage
    {
        /// <summary>Constructor</summary>
        public HopperMotorControl() : base(GdsConstants.ReportId.HopperMotorControl) { }

        /// <summary>Gets or sets the hopper on off state .</summary>
        /// <value>flag to on/off the motor.</value>
        [FieldOrder(0)]
        public bool OnOff { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant($"{GetType()} [OnOff={OnOff}]");
        }
    }
}
