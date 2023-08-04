namespace Aristocrat.Monaco.Hardware.Contracts.Communicator
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Reel.Capabilities;
    using Reel.ControlData;
    using Reel.Events;

    /// <summary>
    ///     Interface that defines a Relm Communicator
    /// </summary>
    [CLSCompliant(false)]
    public interface IRelmCommunicator : ICommunicator,
        IDfuDriver,
        IReelAnimationCapabilities,
        IReelSynchronizationCapabilities,
        IReelBrightnessCapabilities,
        IStepperRuleCapabilities
    {
        /// <summary>
        ///     Gets the default home step.
        /// </summary>
        int DefaultHomeStep { get; }

        /// <summary>
        ///     Event that occurs when component statuses are received.
        /// </summary>
        event EventHandler<ReelStatusReceivedEventArgs> ReelStatusReceived;

        /// <summary>
        ///     The event that occurs when the reel controller has a fault
        /// </summary>
        event EventHandler<ReelControllerFaultedEventArgs> ControllerFaultOccurred;

        /// <summary>
        ///     The event that occurs when the reel controller fault was cleared
        /// </summary>
        event EventHandler<ReelControllerFaultedEventArgs> ControllerFaultCleared;

        /// <summary>
        ///     The event that occurs when a light status is updated
        /// </summary>
        event EventHandler<LightEventArgs> LightStatusReceived;

        /// TODO: Future work will be needed to properly handle interrupts
        /// <summary>
        ///     Event occurs when a reel idle interrupt is received
        /// </summary>
        public event EventHandler<ReelStopData> ReelIdleInterruptReceived;

        /// <summary>
        ///     Initializes the communicator.
        /// </summary>
        Task Initialize();

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
        ///     Requests the statuses from the reel controller.
        /// </summary>
        Task RequestDeviceStatuses();

        /// <summary>
        ///     Instructs the controller to remove all animation files
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        Task<bool> RemoveAllControllerAnimations(CancellationToken token = default);
    }
}
