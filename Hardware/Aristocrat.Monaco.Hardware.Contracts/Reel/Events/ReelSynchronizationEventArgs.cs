namespace Aristocrat.Monaco.Hardware.Contracts.Reel.Events
{
    using System;

    /// <summary>
    ///     The event arguments for reel controller synchronization events
    /// </summary>
    public class ReelSynchronizationEventArgs : EventArgs
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="ReelSynchronizationEventArgs"/> class.
        /// </summary>
        /// <param name="reelId">The reel id</param>
        public ReelSynchronizationEventArgs(int reelId)
        {
            ReelId = reelId;
        }

        /// <summary>
        ///     Gets the reel id.
        /// </summary>
        public int ReelId { get; }
    }
}
