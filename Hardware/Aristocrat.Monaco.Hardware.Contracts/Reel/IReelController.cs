namespace Aristocrat.Monaco.Hardware.Contracts.Reel
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Capabilities;
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

        /// <summary>Get or sets the offsets for each reel</summary>
        IEnumerable<int> ReelOffsets { get; set; }

        /// <summary>Get or sets the home steps for each reel</summary>
        IReadOnlyDictionary<int, int> ReelHomeSteps { get; set; }

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
        ///     Tilts the reels (slow spinning)
        /// </summary>
        /// <returns>Whether or not the reels where tilted</returns>
        Task<bool> TiltReels();

        /// <summary>
        ///     Get all capabilities of the reel controller
        /// </summary>
        /// <returns>An enumeration of capabilities.</returns>
        IEnumerable<Type> GetCapabilities();

        /// <summary>
        ///     Get the implementation for the capability.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The capability for the controller</returns>
        T GetCapability<T>() where T : class, IReelControllerCapability;

        /// <summary>
        ///     Whether or not the reel controller has a given capability.
        /// </summary>
        /// <typeparam name="T">The capability type.</typeparam>
        /// <returns>Return true if the controller has the capability, otherwise false.</returns>
        bool HasCapability<T>() where T : class, IReelControllerCapability;
    }
}