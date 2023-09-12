namespace Aristocrat.Monaco.Hardware.Contracts.Reel.Events
{
    using static System.FormattableString;

    /// <summary>
    ///     The event that is raised when reel synchronization occurs
    /// </summary>
    public class ReelSynchronizationEvent : ReelControllerBaseEvent
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="ReelSynchronizationEvent"/> class.
        /// </summary>
        /// <param name="reelId">The reel id</param>
        /// <param name="status">The synchronization status</param>
        public ReelSynchronizationEvent(int reelId, SynchronizeStatus status)
        {
            ReelId = reelId;
            Status = status;
        }

        /// <summary>
        ///     Gets the reel id
        /// </summary>
        public int ReelId { get; }

        /// <summary>
        ///     Gets the synchronization status
        /// </summary>
        public SynchronizeStatus Status { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"{base.ToString()} [ReelId={ReelId}] [Status={Status}]");
        }
    }
}
