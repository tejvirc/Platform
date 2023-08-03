namespace Aristocrat.Monaco.Hardware.Contracts.Reel.ControlData
{
    using System;

    /// <summary>
    ///     The data associated with a reel controller light show file
    /// </summary>
    [CLSCompliant(false)]
    public class LightShowData
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="LightShowData" /> class.
        /// </summary>
        /// <param name="reelIndex">The reel index for the curve data</param>
        /// <param name="animationName">The name of the animation</param>
        /// <param name="tag">The tag to use</param>
        /// <param name="loopCount">The number of times to loop the animation</param>
        /// <param name="step">The step at which to play the animation</param>
        public LightShowData(sbyte reelIndex, string animationName, string tag, sbyte loopCount, short step)
        {
            ReelIndex = reelIndex;
            AnimationName = animationName;
            Tag = tag;
            LoopCount = loopCount;
            Step = step;
        }

        /// <summary>
        ///     The animation name
        /// </summary>
        public string AnimationName { get; set; }

        /// <summary>
        ///     The tag hash
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        ///     Number of times to loop the animation
        /// </summary>
        public sbyte LoopCount { get; set; }

        /// <summary>
        ///     The reel to apply the animation to
        /// </summary>
        public sbyte ReelIndex { get; set; }

        /// <summary>
        ///     The step at which to play the animation.
        ///     Use -1 if this does not apply
        /// </summary>
        public short Step { get; set; }
    }
}
