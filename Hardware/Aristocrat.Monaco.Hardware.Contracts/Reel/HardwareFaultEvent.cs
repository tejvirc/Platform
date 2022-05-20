namespace Aristocrat.Monaco.Hardware.Contracts.Reel
{
    using System;
    using static System.FormattableString;

    /// <summary> Definition of the HardwareFaultEvent class.</summary>
    [Serializable]
    public class HardwareFaultEvent : ReelControllerBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HardwareFaultEvent"/> class.
        /// </summary>
        /// <param name="fault">The fault.</param>
        public HardwareFaultEvent(ReelControllerFaults fault)
        {
            Fault = fault;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="HardwareFaultEvent"/> class.
        /// </summary>
        /// <param name="reelControllerId">The ID of the reel controller associated with the event.</param>
        /// <param name="fault">The fault.</param>
        public HardwareFaultEvent(int reelControllerId, ReelControllerFaults fault)
            : base(reelControllerId)
        {
            Fault = fault;
        }

        /// <summary>Gets the fault.</summary>
        /// <value>The fault.</value>
        public ReelControllerFaults Fault { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"{base.ToString()} [Fault={Fault}]");
        }
    }
}