namespace Aristocrat.Monaco.Hardware.Contracts.Reel.Events
{
    using System;

    /// <summary>
    ///     Event args for reels stopping
    /// </summary>
    public class ReelStoppingEventArgs : EventArgs
    {
        /// <summary>
        ///     Creates an instance of <see cref="ReelStoppingEventArgs"/>
        /// </summary>
        /// <param name="reelId">The reel Id for this event</param>
        /// <param name="timeToStop">The time the reels are expected to be stopped in</param>
        public ReelStoppingEventArgs(int reelId, long timeToStop)
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
    }
}
