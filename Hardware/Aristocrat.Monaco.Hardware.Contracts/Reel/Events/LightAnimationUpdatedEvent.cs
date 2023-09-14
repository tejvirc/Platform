namespace Aristocrat.Monaco.Hardware.Contracts.Reel.Events
{
    using Kernel;
    using static System.FormattableString;

    /// <summary>
    ///     The event for when a light animation's status is updated
    /// </summary>
    public class LightAnimationUpdatedEvent : BaseEvent
    {
        /// <summary>
        ///     Creates a new instance of <see cref="LightAnimationUpdatedEvent"/>
        /// </summary>
        /// <param name="animationName">the animation identifier</param>
        /// <param name="tag">the tag for the animation</param>
        /// <param name="state">what state the animation is in</param>
        public LightAnimationUpdatedEvent(string animationName, string tag, AnimationState state)
        {
            AnimationName = animationName;
            Tag = tag;
            State = state;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="LightAnimationUpdatedEvent"/>
        /// </summary>
        /// <param name="animationName">the animation identifier</param>
        /// <param name="tag">the tag for the animation</param>
        /// <param name="state">what state the animation is in</param>
        /// <param name="queueType">The queue type</param>
        public LightAnimationUpdatedEvent(string animationName, string tag, AnimationState state, AnimationQueueType queueType)
        {
            AnimationName = animationName;
            Tag = tag;
            State = state;
            QueueType = queueType;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="LightAnimationUpdatedEvent"/>
        /// </summary>
        /// <param name="animationName">the animation identifier</param>
        /// <param name="tag">the tag for the animation</param>
        /// <param name="preparedStatus">The prepared status</param>
        public LightAnimationUpdatedEvent(string animationName, string tag, AnimationPreparedStatus preparedStatus)
        {
            AnimationName = animationName;
            Tag = tag;
            PreparedStatus = preparedStatus;
            State = AnimationState.Prepared;
        }

        /// <summary>
        ///     Gets the animation identifier
        /// </summary>
        public string AnimationName { get; }

        /// <summary>
        ///     Gets the tag for the animation
        /// </summary>
        public string Tag { get; }

        /// <summary>
        ///     Gets the animation state
        /// </summary>
        public AnimationState State { get; }

        /// <summary>
        ///     Gets the animation queue type
        /// </summary>
        public AnimationQueueType QueueType { get; }

        /// <summary>
        ///     Gets the animation prepared status
        /// </summary>
        public AnimationPreparedStatus PreparedStatus { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"{base.ToString()} [AnimationName={AnimationName}] [Tag={Tag}] [State={State}] [QueueType={QueueType}] [PreparedStatus={PreparedStatus}]");
        }
    }
}
