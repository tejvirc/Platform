namespace Aristocrat.Monaco.Hardware.Contracts.Reel
{
    using static System.FormattableString;

    /// <summary>
    ///     The reel stopped event for a given reel
    /// </summary>
    public class ReelStoppedEvent : ReelControllerBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ReelStoppedEvent" /> class.
        /// </summary>
        /// <param name="reelId">the stopped reel id</param>
        /// <param name="step">the step position that the reel stopped on</param>
        /// <param name="isReelStoppedFromHoming">is the reel stopping from homing</param>
        public ReelStoppedEvent(int reelId, int step, bool isReelStoppedFromHoming)
        {
            ReelId = reelId;
            Step = step;
            IsReelStoppedFromHoming = isReelStoppedFromHoming;
        }

        /// <summary>
        ///     The reel Id that stopped
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public int ReelId { get; }

        /// <summary>
        ///     The reel step that the reel has stopped on
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public int Step { get; }

        /// <summary>
        ///     Indicate whether the reel stopped event was from a reel homing or not
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public bool IsReelStoppedFromHoming { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"{base.ToString()} [ReelId={ReelId}] [Step={Step}] [IsReelStoppedFromHoming={IsReelStoppedFromHoming}]");
        }
    }
}