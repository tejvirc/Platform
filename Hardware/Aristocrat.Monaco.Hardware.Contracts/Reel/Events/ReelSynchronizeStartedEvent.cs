namespace Aristocrat.Monaco.Hardware.Contracts.Reel.Events
{
    using Kernel;

    /// <summary>
    ///     Event for when a reel starts synchronizing
    /// </summary>
    public class ReelSynchronizeStartedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ReelSynchronizeStartedEvent" /> class.
        /// </summary>
        /// <param name="reelId">the reel id</param>
        public ReelSynchronizeStartedEvent(int reelId)
        {
            ReelId = reelId;
        }

        /// <summary>
        ///     The reel Id that is accelerating
        /// </summary>
        public int ReelId { get; }
    }
}
