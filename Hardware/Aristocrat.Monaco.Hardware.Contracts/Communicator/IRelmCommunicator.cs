namespace Aristocrat.Monaco.Hardware.Contracts.Communicator
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Reel.Capabilities;
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

        /// <summary>
        ///     Event occurs when a reel idle interrupt is received
        /// </summary>
        public event EventHandler<ReelSpinningEventArgs> ReelSpinningStatusReceived;
        
        /// <summary>
        ///     The event that occurs when the reel begins to stop spinning and idle time is calculated
        /// </summary>
        public event EventHandler<ReelStoppingEventArgs> ReelStopping;

        /// <summary>
        ///     The event that occurs when all light animations are removed from the playing queue 
        /// </summary>
        event EventHandler AllLightAnimationsCleared;
        
        /// <summary>
        ///     The event that occurs when a light animation is removed from the playing queue 
        /// </summary>
        public event EventHandler<LightAnimationEventArgs> LightAnimationRemoved;

        /// <summary>
        ///     The event that occurs when the reel controller starts a light animation
        /// </summary>
        event EventHandler<LightAnimationEventArgs> LightAnimationStarted;

        /// <summary>
        ///     The event that occurs when the reel controller stops a light animation
        /// </summary>
        event EventHandler<LightAnimationEventArgs> LightAnimationStopped;

        /// <summary>
        ///     The event that occurs when the reel controller prepares a light animation
        /// </summary>
        event EventHandler<LightAnimationEventArgs> LightAnimationPrepared;

        /// <summary>
        ///     The event that occurs when the reel controller starts a reel animation
        /// </summary>
        event EventHandler<ReelAnimationEventArgs> ReelAnimationStarted;

        /// <summary>
        ///     The event that occurs when the reel controller stops a reel animation
        /// </summary>
        event EventHandler<ReelAnimationEventArgs> ReelAnimationStopped;

        /// <summary>
        ///     The event that occurs when the reel controller prepares a reels animation
        /// </summary>
        event EventHandler<ReelAnimationEventArgs> ReelAnimationPrepared;

        /// <summary>
        ///     The event that occurs when the reel controller triggers a stepper rule (user event)
        /// </summary>
        event EventHandler<StepperRuleTriggeredEventArgs> StepperRuleTriggered;

        /// <summary>
        ///     The event that occurs when the reel controller starts reel synchronization
        /// </summary>
        event EventHandler<ReelSynchronizationEventArgs> SynchronizationStarted;
        
        /// <summary>
        ///     The event that occurs when the reel controller completes reel synchronization
        /// </summary>
        event EventHandler<ReelSynchronizationEventArgs> SynchronizationCompleted;

        /// <summary>
        ///     Initializes the communicator.
        /// </summary>
        Task Initialize();

        /// <summary>
        ///     Halts the reels (releases brake)
        /// </summary>
        /// <returns>Whether or not the reels were halted</returns>
        Task<bool> HaltReels();

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
        /// <returns>Whether or not the reels were tilted</returns>
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
