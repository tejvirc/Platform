namespace Aristocrat.Monaco.Hardware.Contracts.Reel
{
    using System;
    using static System.FormattableString;

    /// <summary> Definition of the HardwareFaultClearEvent class.</summary>
    [Serializable]
    public class HardwareReelFaultClearEvent : ReelControllerBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HardwareReelFaultClearEvent"/> class.
        /// </summary>
        /// <param name="fault">The fault.</param>
        public HardwareReelFaultClearEvent(ReelFaults fault)
        {
            Fault = fault;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="HardwareReelFaultClearEvent"/> class.
        /// </summary>
        /// <param name="reelControllerId">The ID of the reel controller associated with the event.</param>
        /// <param name="fault">The fault.</param>
        public HardwareReelFaultClearEvent(int reelControllerId, ReelFaults fault)
            : base(reelControllerId)
        {
            Fault = fault;
        }

        /// <summary>Gets the fault.</summary>
        /// <value>The fault.</value>
        public ReelFaults Fault { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"{base.ToString()} [Fault={Fault}]");
        }
    }
}