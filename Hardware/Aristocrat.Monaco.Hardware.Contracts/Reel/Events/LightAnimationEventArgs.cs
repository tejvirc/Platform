namespace Aristocrat.Monaco.Hardware.Contracts.Reel.Events
{
    using System;

    /// <summary>
    ///     The event arguments for reel controller light animation events
    /// </summary>
    public class LightAnimationEventArgs : EventArgs
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="LightAnimationEventArgs"/> class.
        /// </summary>
        /// <param name="animationName">The animation name</param>
        /// <param name="queueType">The animation queue type</param>
        public LightAnimationEventArgs(string animationName, AnimationQueueType queueType)
        {
            AnimationName = animationName;
            QueueType = queueType;
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="LightAnimationEventArgs"/> class.
        /// </summary>
        /// <param name="animationName">The animation name</param>
        /// <param name="tag">The tag</param>
        /// <param name="queueType">The animation queue type</param>
        public LightAnimationEventArgs(string animationName, string tag, AnimationQueueType queueType = AnimationQueueType.Unknown)
        {
            AnimationName = animationName;
            Tag = tag;
            QueueType = queueType;
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="LightAnimationEventArgs"/> class.
        /// </summary>
        /// <param name="animationName">The animation name</param>
        /// <param name="tag">The tag</param>
        /// <param name="preparedStatus">The prepared status</param>
        public LightAnimationEventArgs(string animationName, string tag, AnimationPreparedStatus preparedStatus)
        {
            AnimationName = animationName;
            Tag = tag;
            PreparedStatus = preparedStatus;
        }

        /// <summary>
        ///     Gets the animation name.
        /// </summary>
        public string AnimationName { get; }

        /// <summary>
        ///     Gets the tag.
        /// </summary>
        public string Tag { get; }

        /// <summary>
        ///     Gets the animation queue type
        /// </summary>
        public AnimationQueueType QueueType { get; }

        /// <summary>
        ///     Gets the animation prepared status
        /// </summary>
        public AnimationPreparedStatus PreparedStatus { get; }
    }
}
