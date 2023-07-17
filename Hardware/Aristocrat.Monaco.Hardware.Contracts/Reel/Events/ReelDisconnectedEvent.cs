namespace Aristocrat.Monaco.Hardware.Contracts.Reel.Events
{
    using static System.FormattableString;

    /// <summary>
    ///     The disconnected event for a given reel
    /// </summary>
    public class ReelDisconnectedEvent : ReelControllerBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ReelDisconnectedEvent" /> class.
        /// </summary>
        /// <param name="reelId">the disconnected reel id</param>
        public ReelDisconnectedEvent(int reelId)
        {
            ReelId = reelId;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ReelDisconnectedEvent" /> class.
        /// </summary>
        /// <param name="reelId">the disconnected reel id</param>
        /// <param name="reelControllerId">The associated reel controller ID.</param>
        public ReelDisconnectedEvent(int reelId, int reelControllerId)
            : base(reelControllerId)
        {
            ReelId = reelId;
        }

        /// <summary>
        ///     The reel Id that was disconnected
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public int ReelId { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"{base.ToString()} [ReelId={ReelId}]");
        }
    }
}