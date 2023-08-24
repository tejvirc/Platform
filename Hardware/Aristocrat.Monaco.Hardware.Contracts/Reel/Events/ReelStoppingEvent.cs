namespace Aristocrat.Monaco.Hardware.Contracts.Reel.Events
{
    using static System.FormattableString;

    /// <summary>
    /// 
    /// </summary>
    public class ReelStoppingEvent: ReelControllerBaseEvent
    {
        /// <summary>
        ///     Creates an instance of <see cref="ReelStoppingEvent"/>
        /// </summary>
        /// <param name="reelId">The reel Id for this event</param>
        /// <param name="timeToStop">The time the reels are expected to be stopped in</param>
        public ReelStoppingEvent(int reelId, long timeToStop)
        {
            ReelId = reelId;
            TimeToStop = timeToStop;
        }

        /// <summary>
        ///     Gets the reel Id for the event
        /// </summary>
        public int ReelId { get; }

        /// <summary>
        ///     Gets the time to stop
        /// </summary>
        public long TimeToStop { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"{base.ToString()} [ReelId={ReelId}] [IdleTime={TimeToStop}]");
        }
    }
}
