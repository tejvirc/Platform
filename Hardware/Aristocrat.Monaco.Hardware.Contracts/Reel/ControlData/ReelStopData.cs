namespace Aristocrat.Monaco.Hardware.Contracts.Reel.ControlData
{
    /// <summary>
    ///     The data required for stopping reels.
    /// </summary>
    public class ReelStopData
    {
        /// <summary>
        ///     Creates a new instance of ReelStopData
        /// </summary>
        /// <param name="reelIndex">The reel index, based at 0</param>
        /// <param name="duration">time in milliseconds the reel will try to stop</param>
        /// <param name="step">what step to stop at</param>
        public ReelStopData(byte reelIndex, short duration, short step)
        {
            ReelIndex = reelIndex;
            Duration = duration;
            Step = step;
        }

        /// <summary>
        ///     This value indicates the index of the reel to stop. It must be within the range of [0,detectedReelNumber – 1].
        /// </summary>
        public byte ReelIndex { get; set; }

        /// <summary>
        ///     This value indicates the duration of the complete stop in milliseconds. If the required
        ///     reel cannot stop within this duration, the reel controller firmware tries to stop the
        ///     reel as quick as possible.
        /// </summary>
        public short Duration { get; set; }

        /// <summary>
        ///     This value indicates the step position at which to stop. Note: If the expected value is
        ///     located on the edge of a tab, a USB pipe error occurs.
        /// </summary>
        public short Step { get; set; }
    }
}
