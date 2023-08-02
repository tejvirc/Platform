namespace Aristocrat.Monaco.Hardware.Contracts.Reel.Events
{
    using System;
    using Kernel;

    /// <summary>
    ///     The event for when a reel animation's status is updated
    /// </summary>
    [CLSCompliant(false)]
    public class ReelAnimationUpdatedEvent : BaseEvent
    {
        /// <summary>
        /// Creates a new instance of <see cref="ReelAnimationUpdatedEvent">
        /// </summary>
        /// <param name="animationName">the animation identifier</param>
        /// <param name="reelIndex">what reel this event is for</param>
        /// <param name="state">what state the animation is in</param>
        public ReelAnimationUpdatedEvent(string animationName, uint reelIndex, AnimationState state)
        {
            AnimationName = animationName;
            ReelIndex = reelIndex;
            State = state;
        }

        /// <summary>
        ///     the animation identifier
        /// </summary>
        public string AnimationName { get; }

        /// <summary>
        ///     what reel this event is for
        /// </summary>
        public uint ReelIndex { get; }

        /// <summary>
        ///     what state the animation is in
        /// </summary>
        public AnimationState State { get; }
    }
}
