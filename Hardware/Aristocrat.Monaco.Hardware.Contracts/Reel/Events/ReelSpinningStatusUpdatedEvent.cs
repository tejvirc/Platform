namespace Aristocrat.Monaco.Hardware.Contracts.Reel.Events
{
    using static System.FormattableString;

    /// <summary>
    ///     The reel accelerating event for a given reel
    /// </summary>
    public class ReelSpinningStatusUpdatedEvent : ReelControllerBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ReelSpinningStatusUpdatedEvent" /> class.
        /// </summary>
        /// <param name="reelId">the reel id</param>
        /// <param name="spinVelocity">the reel spin velocity of the reel/param>
        public ReelSpinningStatusUpdatedEvent(int reelId, SpinVelocity spinVelocity)
        {
            ReelId = reelId;
            SpinVelocity = spinVelocity;
        }

        /// <summary>
        ///     The reel Id that is accelerating
        /// </summary>
        public int ReelId { get; }

        /// <summary>
        ///     The reel spin velocity of the reel
        /// </summary>
        public SpinVelocity SpinVelocity { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"{base.ToString()} [ReelId={ReelId}] [SpinVelocity={SpinVelocity}]");
        }
    }
}