namespace Aristocrat.Monaco.Gaming.Commands
{
    public class StopAllAnimationTags
    {
        /// <summary>
        /// The animation to stop
        /// </summary>
        public string AnimationName { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether or not all animation with the identifier were stopped
        /// </summary>
        public bool Success { get; set; }
    }
}
