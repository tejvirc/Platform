namespace Aristocrat.Monaco.Hardware.Contracts.Reel
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Gds.Reel;

    /// <summary>
    ///     The reel controller implementation
    /// </summary>
    public interface IReelControllerImplementation : IGdsDevice
    {
        /// <summary> The event that occurs when the reel controller has a fault </summary>
        event EventHandler<ReelControllerFaultedEventArgs> ControllerFaultOccurred;

        /// <summary> The event that occurs when the reel controller fault was cleared </summary>
        event EventHandler<ReelControllerFaultedEventArgs> ControllerFaultCleared;

        /// <summary> The event that occurs when the reel has a fault </summary>
        event EventHandler<ReelFaultedEventArgs> FaultOccurred;

        /// <summary> The event that occurs whens the reel fault was cleared </summary>
        event EventHandler<ReelFaultedEventArgs> FaultCleared;

        /// <summary> The event that occurs when the reel stops spinning </summary>
        event EventHandler<ReelEventArgs> ReelStopped;

        /// <summary> The event that occurs when the reel starts spinning </summary>
        event EventHandler<ReelEventArgs> ReelSpinning;

        /// <summary> The event that occurs when the reel starts slow spinning </summary>
        event EventHandler<ReelEventArgs> ReelSlowSpinning;

        /// <summary> The event that occurs when a reel is disconnected </summary>
        event EventHandler<ReelEventArgs> ReelDisconnected;

        /// <summary> The event that occurs when a reel is connected </summary>
        event EventHandler<ReelEventArgs> ReelConnected;
        
        /// <summary> The event that occurs when a reel controller hardware is fully initialized</summary>
        event EventHandler HardwareInitialized;

        /// <summary> Gets the collection of reels IDs for this controller </summary>
        IReadOnlyCollection<int> ReelIds { get; }

        /// <summary>Gets the reel controller faults.</summary>
        ReelControllerFaults ReelControllerFaults { get; }

        /// <summary>Gets the reel faults.</summary>
        IReadOnlyDictionary<int, ReelFaults> Faults { get; }

        /// <summary> Gets the status for each of the available reels </summary>
        IReadOnlyDictionary<int, ReelStatus> ReelsStatus { get; }

        /// <summary>
        ///     Homes the reel to the requested stop
        /// </summary>
        /// <param name="reelId">The reel ID to home</param>
        /// <param name="stop">The stop to home the reel to</param>
        /// <param name="resetStatus">Indicates whether or not to reset the status for this reel</param>
        /// <returns>Whether or not the reel was homed</returns>
        Task<bool> HomeReel(int reelId, int stop, bool resetStatus = true);

        /// <summary>
        ///     Homes the reels to the requested stop
        /// </summary>
        /// <returns>Whether or not the reels were homed</returns>
        Task<bool> HomeReels();

        /// <summary>
        ///     Spins the reels with the requested spin data
        /// </summary>
        /// <param name="reelData">The spin data to use</param>
        /// <returns>Whether or not the reels were started spinning</returns>
        Task<bool> SpinReels(params ReelSpinData[] reelData);

        /// <summary>
        ///     Nudges the reels with the requested nudge data
        /// </summary>
        /// <param name="reelData">The nudge data to use</param>
        /// <returns>Whether or the not reels were nudged/returns>
        Task<bool> NudgeReels(params NudgeReelData[] reelData);

        /// <summary>
        ///     Sets the brightness for the specified reel lights
        /// </summary>
        /// <param name="brightness">The reel and brightness to set for the lights</param>
        /// <returns>Whether or not the light brightness was set</returns>
        Task<bool> SetBrightness(IReadOnlyDictionary<int, int> brightness);

        /// <summary>
        ///     Sets the brightness for the reel lights
        /// </summary>
        /// <param name="brightness">The brightness to set for the lights</param>
        /// <returns>Whether or not the light brightness was set</returns>
        Task<bool> SetBrightness(int brightness);

        /// <summary>
        ///     Sets the speed for all reels
        /// </summary>
        /// <param name="speedData">The speed data to set for the reels</param>
        /// <returns>Whether or not the speed was set</returns>
        Task<bool> SetReelSpeed(params ReelSpeedData[] speedData);

        /// <summary>
        ///     Sets the light state and color
        /// </summary>
        /// <param name="lampData">The lamp data to set for the reels</param>
        /// <returns>Whether or not the light was set</returns>
        Task<bool> SetLights(params ReelLampData[] lampData);

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
        ///     Requests a list of reel light identifiers for the reel controller.
        /// </summary>
        /// <returns>A list of reel light identifiers for the reel controller</returns>
        Task<IList<int>> GetReelLightIdentifiers();
    }
}