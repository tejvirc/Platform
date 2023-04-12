namespace Aristocrat.Monaco.Hardware.Contracts.Reel
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Events;
    using Gds.Reel;
    using ImplementationCapabilities;

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

        /// <summary> The event that occurs when the reel begins to stop spinning </summary>
        event EventHandler<ReelEventArgs> ReelStopping;

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
        ///     Get all capabilities of the reel controller
        /// </summary>
        /// <returns>An enumeration of capabilities.</returns>
        IEnumerable<Type> GetCapabilities();

        /// <summary>
        ///     Get the implementation for the capability.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The capability for the controller</returns>
        T GetCapability<T>() where T : class, IReelImplementationCapability;

        /// <summary>
        ///     Whether or not the reel controller has a given capability.
        /// </summary>
        /// <typeparam name="T">The capability type.</typeparam>
        /// <returns>Return true if the controller has the capability, otherwise false.</returns>
        bool HasCapability<T>() where T : class, IReelImplementationCapability;
    }
}