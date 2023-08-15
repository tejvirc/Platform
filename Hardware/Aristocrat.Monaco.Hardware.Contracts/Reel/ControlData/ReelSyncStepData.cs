namespace Aristocrat.Monaco.Hardware.Contracts.Reel.ControlData
{
    /// <summary>
    ///     The <see cref="ReelSyncStepData"> class.
    ///     Holds sync data for a specific reel.
    /// </summary>
    public class ReelSyncStepData
    {
        /// <summary>
        ///     Creates a new instance of <see cref="ReelSyncStepData">.
        /// </summary>
        /// <param name="reelIndex">The index of the reel</param>
        /// <param name="syncStep">The step position on which to synchronize.</param>
        /// <param name="duration">The duration in milliseconds. (Only used for Enhanced Synchronize)</param>
        public ReelSyncStepData(byte reelIndex, short syncStep, short duration = 0)
        {
            ReelIndex = reelIndex;
            SyncStep = syncStep;
            Duration = duration;
        }

        /// <summary>
        ///     This value indicates the index of the reel.
        ///     It must be within the range of [0,detectedReelNumber – 1].
        /// </summary>
        public byte ReelIndex { get; set; }

        /// <summary>
        ///     This value indicates the step position on which to synchronize.
        /// </summary>
        public short SyncStep { get; set; }

        /// <summary>
        ///     The duration to sync in milliseconds. (Only used for Enhanced Synchronize)
        /// </summary>
        public short Duration { get; set; }
    }
}
