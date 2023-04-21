namespace Aristocrat.Monaco.Hardware.Contracts.Reel.Events
{
    using System;
    using static System.FormattableString;

    /// <summary> Definition of the HardwareReelFaultEvent class.</summary>
    [Serializable]
    public class HardwareReelFaultEvent : ReelControllerBaseEvent
    {
        private const int UnknownReelId = 0;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HardwareReelFaultEvent"/> class.
        /// </summary>
        /// <param name="fault">The fault.</param>
        /// <param name="reelId">The reel Id that faulted.</param>
        public HardwareReelFaultEvent(ReelFaults fault, int reelId = UnknownReelId)
        {
            Fault = fault;
            ReelId = reelId;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="HardwareReelFaultEvent"/> class.
        /// </summary>
        /// <param name="reelControllerId">The ID of the reel controller associated with the event.</param>
        /// <param name="fault">The fault.</param>
        /// <param name="reelId">The reel Id that faulted.</param>
        public HardwareReelFaultEvent(int reelControllerId, ReelFaults fault, int reelId = UnknownReelId)
            : base(reelControllerId)
        {
            Fault = fault;
            ReelId = reelId;
        }

        /// <summary>Gets the fault.</summary>
        /// <value>The fault.</value>
        public ReelFaults Fault { get; }

        /// <summary>
        ///     The reel Id that faulted.  A value of 0 is an unknown reel.
        /// </summary>
        public int ReelId { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"{base.ToString()} [Fault={Fault}] [ReelId={ReelId}]");
        }
    }
}