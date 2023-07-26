namespace Aristocrat.Monaco.Hardware.Contracts.Reel.Events
{
    using static System.FormattableString;

    /// <summary>
    ///     The reel accelerating event for a given reel
    /// </summary>
    public class ReelAcceleratingEvent : ReelControllerBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ReelAcceleratingEvent" /> class.
        /// </summary>
        /// <param name="reelId">the reel id</param>
        public ReelAcceleratingEvent(int reelId)
        {
            ReelId = reelId;
        }

        /// <summary>
        ///     The reel Id that is accelerating
        /// </summary>
        public int ReelId { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"{base.ToString()} [ReelId={ReelId}]");
        }
    }
}