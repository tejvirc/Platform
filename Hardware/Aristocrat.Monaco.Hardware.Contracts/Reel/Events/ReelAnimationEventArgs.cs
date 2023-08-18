namespace Aristocrat.Monaco.Hardware.Contracts.Reel.Events
{
    using System;

    /// <summary>
    ///     The event arguments for reel controller animation events
    /// </summary>
    public class ReelAnimationEventArgs : EventArgs
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="ReelAnimationEventArgs"/> class.
        /// </summary>
        /// <param name="animationName">The animation name</param>
        /// <param name="preparedStatus">The prepared status</param>
        public ReelAnimationEventArgs(string animationName, AnimationPreparedStatus preparedStatus)
        {
            AnimationName = animationName;
            PreparedStatus = preparedStatus;
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="ReelAnimationEventArgs"/> class.
        /// </summary>
        /// <param name="reelId">The reel id</param>
        /// <param name="animationName">The animation name</param>
        public ReelAnimationEventArgs(int reelId, string animationName)
        {
            ReelId = reelId;
            AnimationName = animationName;
        }

        /// <summary>
        ///     Gets the animation name.
        /// </summary>
        public string AnimationName { get; }

        /// <summary>
        ///     Gets the reel id.
        /// </summary>
        public int ReelId { get; }

        /// <summary>
        ///     Gets the animation prepared status
        /// </summary>
        public AnimationPreparedStatus PreparedStatus { get; }
    }
}
