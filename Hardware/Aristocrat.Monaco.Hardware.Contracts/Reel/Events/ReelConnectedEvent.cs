namespace Aristocrat.Monaco.Hardware.Contracts.Reel.Events
{
    using static System.FormattableString;

    /// <summary>
    ///     The connected event for a given reel
    /// </summary>
    public class ReelConnectedEvent : ReelControllerBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ReelConnectedEvent" /> class.
        /// </summary>
        /// <param name="reelId">the connected reel id</param>
        public ReelConnectedEvent(int reelId)
        {
            ReelId = reelId;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ReelConnectedEvent" /> class.
        /// </summary>
        /// <param name="reelId">the connected reel id</param>
        /// <param name="reelControllerId">The associated reel controller ID.</param>
        public ReelConnectedEvent(int reelId, int reelControllerId)
            : base(reelControllerId)
        {
            ReelId = reelId;
        }

        /// <summary>
        ///     The reel Id that was connected
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