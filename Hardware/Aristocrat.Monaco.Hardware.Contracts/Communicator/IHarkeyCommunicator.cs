namespace Aristocrat.Monaco.Hardware.Contracts.Communicator
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Aristocrat.Monaco.Hardware.Contracts.Reel.ControlData;
    using Reel.Events;

    /// <summary>
    ///     Interface that defines a Harkey Communicator
    /// </summary>
    [CLSCompliant(false)]
    public interface IHarkeyCommunicator : ICommunicator
    {
        /// <summary>
        ///     Event that occurs when component statuses are received.
        /// </summary>
        public event EventHandler<ReelStatusReceivedEventArgs> StatusesReceived;

        /// <summary>
        ///     Event that occurs when Reels are tilted on the simulator
        /// </summary>
        public event EventHandler<ReelEventArgs> ReelTilted;

        /// <summary>
        ///     Event that occurs when Reels are spinning on the simulator
        /// </summary>
        public event EventHandler<ReelEventArgs> ReelSpinning;

        /// <summary>
        ///     Event that occurs when Reels are stopped on the simulator
        /// </summary>
        public event EventHandler<ReelEventArgs> ReelStopped;

        /// <summary>
        ///     Initializes the communicator.
        /// </summary>
        Task Initialize();

        /// <summary>
        ///     Homes the reels to the requested stop
        /// </summary>
        /// <returns>Whether or not the reels were homed</returns>
        Task<bool> HomeReels();

        /// <summary>
        ///     Homes the reel to the requested stop
        /// </summary>
        /// <param name="reelId">The reel ID to home</param>
        /// <param name="stop">The stop to home the reel to</param>
        /// <param name="resetStatus">Indicates whether or not to reset the status for this reel</param>
        /// <returns>Whether or not the reel was homed</returns>
        Task<bool> HomeReel(int reelId, int stop, bool resetStatus = true);

        /// <summary>
        ///     Sets the offsets for all reels
        /// </summary>
        /// <param name="offsets">The offsets to set for the reels</param>
        /// <returns>Whether or not the offsets were set</returns>
        Task<bool> SetReelOffsets(params int[] offsets);

        /// <summary>
        ///     Tilts the reels (slow spinning)
        /// </summary>
        /// <returns>Whether or not the reels where tilted</returns>
        Task<bool> TiltReels();

        /// <summary>
        ///     Nudges the reels 
        /// </summary>
        /// <returns>Whether or not the reels where nudged</returns>
        Task<bool> NudgeReels();

        /// <summary>
        ///     Nudges the reels 
        /// </summary>
        /// <returns>Whether or not the reels where nudged</returns>
        Task<bool> NudgeReels(params NudgeReelData[] nudgeData);

        /// <summary>
        ///     Spins the reels 
        /// </summary>
        /// <returns>Whether or not the reels where spun </returns>
        Task<bool> SpinReels();

        /// <summary>
        ///     Spins the reels 
        /// </summary>
        /// <returns>Whether or not the reels where spun </returns>
        Task<bool> SpinReels(params ReelSpinData[] spinReelData);

        /// <summary>
        ///     Requests the statuses from the reel controller.
        /// </summary>
        Task RequestDeviceStatuses();

        /// <summary>
        ///     Sets the brightness for the reel controller.
        /// </summary>
        Task<bool> SetBrightness(int brightness);

        /// <summary>
        ///    Sets the brightness for the reel controller.
        /// </summary>
        Task<bool> SetBrightness(IReadOnlyDictionary<int, int> brightness);

        /// <summary>
        ///    Sets the lights for the reel controller.
        /// </summary>
        Task<bool> SetLights(ReelLampData[] lampData);
    }
}