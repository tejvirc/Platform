namespace Aristocrat.Monaco.Hardware.Contracts.Reel.ControlData
{
    using System;

    /// <summary>
    ///     The data associated with a reel curve
    /// </summary>
    [CLSCompliant(false)]
    public class ReelCurveData
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ReelCurveData" /> class.
        /// </summary>
        /// <param name="reelIndex">The reel index for the curve data</param>
        /// <param name="animationName">The name of the animation</param>
        public ReelCurveData(byte reelIndex, string animationName)
        {
            ReelIndex = reelIndex;
            AnimationName = animationName;
        }

        /// <summary>
        ///     The friendly name of the animation
        /// </summary>
        public string AnimationName { get; set; }

        /// <summary>
        ///     This value indicates the index of the reel for this animation.
        /// </summary>
        public byte ReelIndex { get; set; }
    }
}
