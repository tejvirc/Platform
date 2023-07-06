namespace Aristocrat.Monaco.Gaming.Commands
{
    /// <summary>
    ///     The StopAllAnimationTags class
    /// </summary>
    public class StopAllAnimationTags
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="StopAllAnimationTags" /> class.
        /// </summary>
        /// <param name="animationName">The animation name</param>
        public StopAllAnimationTags(string animationName)
        {
            AnimationName = animationName;
        }

        /// <summary>
        ///     The animation to stop
        /// </summary>
        public string AnimationName { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether or not all animation with the identifier were stopped
        /// </summary>
        public bool Success { get; set; }
    }
}
