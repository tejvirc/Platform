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
        /// <param name="direction">the direction</param>
        public ReelDeceleratingEvent(int reelId, SpinDirection direction)
        {
            ReelId = reelId;
            Direction = direction;
        }

        /// <summary>
        ///     The reel Id that is decelerating
        /// </summary>
        public int ReelId { get; }

        /// <summary>
        ///     The direction of the deceleration
        /// </summary>
        public SpinDirection Direction { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"{base.ToString()} [ReelId={ReelId}]");
        }
    }
}