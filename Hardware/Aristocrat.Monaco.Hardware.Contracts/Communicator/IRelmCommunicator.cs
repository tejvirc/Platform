namespace Aristocrat.Monaco.Hardware.Contracts.Communicator
{
    using System;
    using System.Threading.Tasks;
    using Reel.Capabilities;

    /// <summary>
    ///     Interface that defines a Relm Communicator
    /// </summary>
    [CLSCompliant(false)]
    public interface IRelmCommunicator : ICommunicator,
        IDfuDriver,
        IReelAnimationCapabilities,
        IReelSynchronizationCapabilities,
        IReelBrightnessCapabilities
    {
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
    }
}
