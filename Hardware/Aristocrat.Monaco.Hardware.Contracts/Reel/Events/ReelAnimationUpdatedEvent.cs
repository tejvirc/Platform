namespace Aristocrat.Monaco.Hardware.Contracts.Reel.Events
{
    using System;
    using Kernel;
    using static System.FormattableString;

    /// <summary>
    ///     The event for when a reel animation's status is updated
    /// </summary>
    [CLSCompliant(false)]
    public class ReelAnimationUpdatedEvent : BaseEvent
    {
        /// <summary>
        /// Creates a new instance of <see cref="ReelAnimationUpdatedEvent">
        /// </summary>
        /// <param name="reelId">The reel id</param>
        /// <param name="animationName">the animation identifier</param>
        /// <param name="state">what state the animation is in</param>
        public ReelAnimationUpdatedEvent(int reelId, string animationName, AnimationState state)
        {
            ReelId = reelId;
            AnimationName = animationName;
            State = state;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="ReelAnimationUpdatedEvent"/>
        /// </summary>
        /// <param name="animationName">the animation identifier</param>
        /// <param name="preparedStatus">The prepared status</param>
        public ReelAnimationUpdatedEvent(string animationName, AnimationPreparedStatus preparedStatus)
        {
            AnimationName = animationName;
            PreparedStatus = preparedStatus;
            State = AnimationState.Prepared;
        }

        /// <summary>
        ///     Gets the reel id
        /// </summary>
        public int ReelId { get; }

        /// <summary>
        ///     Gets the animation name
        /// </summary>
        public string AnimationName { get; }

        /// <summary>
        ///     Gets the animation state
        /// </summary>
        public AnimationState State { get; }

        /// <summary>
        ///     Gets the animation prepared status
        /// </summary>
        public AnimationPreparedStatus PreparedStatus { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"{base.ToString()} [ReelId={ReelId}] [AnimationName={AnimationName}] [State={State}] [PreparedStatus={PreparedStatus}]");
        }
    }
}
