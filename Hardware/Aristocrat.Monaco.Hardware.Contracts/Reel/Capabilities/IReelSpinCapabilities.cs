namespace Aristocrat.Monaco.Hardware.Contracts.Reel.Capabilities
{
    using System.Threading.Tasks;
    using ControlData;

    /// <summary>
    ///     The public interface for reel controller spin capabilities
    /// </summary>
    public interface IReelSpinCapabilities : IReelControllerCapability
    {
        /// <summary>
        ///     Gets the default speed for reels to spin.
        /// </summary>
        public int DefaultSpinSpeed { get;  }

        /// <summary>
        ///     Nudges the reels with the requested nudge data
        /// </summary>
        /// <param name="reelData">The nudge data to use</param>
        /// <returns>Whether or the not reels were nudged/returns>
        Task<bool> NudgeReels(params NudgeReelData[] reelData);

        /// <summary>
        ///     Spins the reels with the requested spin data
        /// </summary>
        /// <param name="reelData">The spin data to use</param>
        /// <returns>Whether or not the reels were started spinning</returns>
        Task<bool> SpinReels(params ReelSpinData[] reelData);

        /// <summary>
        ///     Sets the speed for all reels
        /// </summary>
        /// <param name="speedData">The reel speed data</param>
        /// <returns>Whether or not the reel speed was updated</returns>
        Task<bool> SetReelSpeed(params ReelSpeedData[] speedData);
    }
}
