namespace Aristocrat.Monaco.Gaming.Commands
{
    using NudgeReelData = Hardware.Contracts.Reel.ControlData.NudgeReelData;

    /// <summary>
    ///     Nudge the mechanical reels
    /// </summary>
    public class NudgeReels
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="NudgeReels" /> class.
        /// </summary>
        /// <param name="nudgeSpinData">The reel nudge spin data</param>
        public NudgeReels(params NudgeReelData[] nudgeSpinData)
        {
            NudgeSpinData = nudgeSpinData;
        }

        /// <summary>
        ///     Gets the reel nudge spin data
        /// </summary>
        public NudgeReelData[] NudgeSpinData { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether or not the reel can nudge
        /// </summary>
        public bool Success { get; set; }
    }
}