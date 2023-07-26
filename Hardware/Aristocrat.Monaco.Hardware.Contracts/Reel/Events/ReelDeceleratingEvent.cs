namespace Aristocrat.Monaco.Hardware.Contracts.Reel.Events
{
    using static System.FormattableString;

    /// <summary>
    ///     The reel decelerating event for a given reel
    /// </summary>
    public class ReelDeceleratingEvent : ReelControllerBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ReelDeceleratingEvent" /> class.
        /// </summary>
        /// <param name="reelId">the reel id</param>
        public ReelDeceleratingEvent(int reelId)
        {
            ReelId = reelId;
        }

        /// <summary>
        ///     The reel Id that is decelerating
        /// </summary>
        public int ReelId { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"{base.ToString()} [ReelId={ReelId}]");
        }
    }
}