namespace Aristocrat.Monaco.Hardware.Contracts.Reel
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Gds.Reel;
    using SharedDevice;

    /// <summary>
    /// </summary>
    public interface IReelController : IDeviceAdapter
    {
        /// <summary>Gets the faults on the reel controller.</summary>
        ReelControllerFaults ReelControllerFaults { get; }

        /// <summary>Gets the reel faults.</summary>
        IReadOnlyDictionary<int, ReelFaults> Faults { get; }

        /// <summary> Gets the states for each of the available reels </summary>
        IReadOnlyDictionary<int, ReelLogicalState> ReelStates { get; }

        /// <summary> Gets the status for each of the available reels </summary>
        IReadOnlyDictionary<int, ReelStatus> ReelsStatus { get; }

        /// <summary> Gets the steps for each of the available reels </summary>
        IReadOnlyDictionary<int, int> Steps { get; }

        /// <summary> Gets the logical state for the reel controller </summary>
        ReelControllerState LogicalState { get; }

        /// <summary> Gets the collection of connected reel IDs for this controller </summary>
        IReadOnlyCollection<int> ConnectedReels { get; }

        /// <summary> Gets the identifier for the reel controller </summary>
        int ReelControllerId { get; }

        /// <summary>Get or sets the default brightness of the reel lights</summary>
        int DefaultReelBrightness { get; set; }

        /// <summary>Get or sets the offsets for each reel</summary>
        IEnumerable<int> ReelOffsets { get; set; }

        /// <summary>Get or sets the home steps for each reel</summary>
        IReadOnlyDictionary<int, int> ReelHomeSteps { get; set; }

        /// <summary>
        ///     Spins the reels with the requested spin data
        /// </summary>
        /// <param name="reelData">The spin data to use</param>
        /// <returns>Whether or not the reels were started spinning</returns>
        Task<bool> SpinReels(params ReelSpinData[] reelData);

        /// <summary>
        ///     Sets the requested lamp data
        /// </summary>
        /// <param name="lampData">The lamp data to set for the reels</param>
        /// <returns>Whether or not the lamps were updated</returns>
        Task<bool> SetLights(params ReelLampData[] lampData);

        /// <summary>
        ///     Sets the brightness for the reel lights
        /// </summary>
        /// <param name="brightness">The brightness to set for the lights</param>
        /// <returns>Whether or not the lamp brightness was updated</returns>
        Task<bool> SetReelBrightness(IReadOnlyDictionary<int, int> brightness);

        /// <summary>
        ///     Sets the brightness for the all reel lights
        /// </summary>
        /// <param name="brightness">The brightness to set for the lights</param>
        /// <returns>Whether or not the lamp brightness was updated</returns>
        Task<bool> SetReelBrightness(int brightness);

        /// <summary>
        ///     Sets the speed for all reels
        /// </summary>
        /// <param name="speedData">The reel speed data</param>
        /// <returns>Whether or not the reel speed was updated</returns>
        Task<bool> SetReelSpeed(params ReelSpeedData[] speedData);

        /// <summary>
        ///     Homes the reels
        /// </summary>
        /// <returns>Whether or not the reels were homed</returns>
        Task<bool> HomeReels();

        /// <summary>
        ///     Homes the specified reels to the requested steps
        /// </summary>
        /// <param name="reelOffsets">The collection of reel Ids to steps to home the reels to</param>
        /// <returns>Whether or not the reels were homed</returns>
        Task<bool> HomeReels(IReadOnlyDictionary<int, int> reelOffsets);

        /// <summary>
        ///     Nudges the reels with the requested nudge data
        /// </summary>
        /// <param name="reelData">The nudge data to use</param>
        /// <returns>Whether or the not reels were nudged/returns>
        Task<bool> NudgeReel(params NudgeReelData[] reelData);

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