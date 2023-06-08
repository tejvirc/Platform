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
        ///     The friendly name of the animation
        /// </summary>
        public string AnimationName { get; set; }

        /// <summary>
        ///     This value indicates the index of the reel for this animation.
        /// </summary>
        public byte ReelIndex { get; set; }
    }
}
