namespace Aristocrat.Monaco.Hardware.Contracts.Reel.Events
{
    using Kernel;

    /// <summary>
    ///     The event for when a reel is synchronized
    /// </summary>
    public class ReelSynchronizedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ReelSynchronizedEvent" /> class.
        /// </summary>
        /// <param name="reelId">the reel id</param>
        public ReelSynchronizedEvent(int reelId)
        {
            ReelId = reelId;
        }

        /// <summary>
        ///     The reel Id that is accelerating
        /// </summary>
        public int ReelId { get; }
    }
}
