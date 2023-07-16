namespace Aristocrat.Monaco.Hardware.Contracts.Reel.Events
{
    using Kernel;

    /// <summary>
    ///     The event for when a light show animation's status is updated
    /// </summary>
    public class LightShowAnimationUpdatedEvent : BaseEvent
    {
        /// <summary>
        /// Creates a new instance of LightShowAnimationStarted
        /// </summary>
        /// <param name="animationName">the animation identifier</param>
        /// <param name="tag">the tag for the animation</param>
        /// <param name="state">what state the animation is in</param>
        public LightShowAnimationUpdatedEvent(string animationName, string tag, AnimationState state)
        {
            AnimationName = animationName;
            Tag = tag;
            State = state;
        }

        /// <summary>
        ///     the animation identifier
        /// </summary>
        public string AnimationName { get; }

        /// <summary>
        ///     the tag for the animation
        /// </summary>
        public string Tag { get; }

        /// <summary>
        ///     what state the animation is in
        /// </summary>
        public AnimationState State { get; }
    }
}
