namespace Aristocrat.Monaco.Hardware.Contracts.Reel
{
    using Kernel;
    using System;

    /// <summary>
    ///     The different types of faults that can occur on a reel controller
    /// </summary>
    [Flags]
    public enum ReelControllerFaults
    {
        /// <summary>
        ///     The state for when there are no faults on the reel controller
        /// </summary>
        None = 0,

        /// <summary>
        ///     The error indicating the reel controller was disconnected
        /// </summary>
        [ErrorGuid("{028C86C2-1A11-464B-897C-88F9FA9F8733}", DisplayableMessageClassification.HardError)]
        Disconnected = 0x0001,

        /// <summary>
        ///     The error indicating a firmware fault on the reel controller
        /// </summary>
        [ErrorGuid("{F7E5E84A-F7FB-4330-BCDB-B980487AFB5D}", DisplayableMessageClassification.HardError)]
        FirmwareFault = 0x0002,

        /// <summary>
        ///     The error indicating a hardware event on the reel controller.  A hardware event
        ///     indicates an issue with the hardware which may be able to be cleared in software.
        /// </summary>
        [ErrorGuid("{26E72A20-E3EA-48F5-BE66-204D7014714D}", DisplayableMessageClassification.HardError)]
        HardwareError = 0x0004,

        /// <summary>
        ///     The error indicating the lights/lamps have an issue
        /// </summary>
        [ErrorGuid("{858DDE61-5BEB-4A82-B246-2D31E579EA39}", DisplayableMessageClassification.HardError)]
        LightError = 0x0008,

        /// <summary>
        ///     The error indicating that the voltage level is invalid for the reel controller
        /// </summary>
        [ErrorGuid("{1716476D-5C53-4A25-B4B2-44F80FA02BB3}", DisplayableMessageClassification.HardError)]
        LowVoltage = 0x0010,

        /// <summary>
        ///     The error indicating a communication error on the reel controller.
        /// </summary>
        [ErrorGuid("{D7C35CBB-9D7F-4C0D-9792-0C27E2E594D0}", DisplayableMessageClassification.HardError)]
        CommunicationError = 0x0020,

        /// <summary>A command request received a NAK response to a well-formed command.</summary>
        [ErrorGuid("{77594C12-3F17-4872-97F3-DD62129D15E5}", DisplayableMessageClassification.HardError)]
        RequestError = 0x0040,
    }

    /// <summary>
    ///     The different types of faults that can occur on a reel for a reel controller
    /// </summary>
    [Flags]
    public enum ReelFaults
    {
        /// <summary>
        ///     The state for when there is no faults on reels
        /// </summary>
        None = 0,

        /// <summary>
        ///     The error indicating a reel was disconnected
        /// </summary>
        [ErrorGuid("{A69CB751-EAD8-4AC2-83F8-70653538EA15}", DisplayableMessageClassification.HardError)]
        Disconnected = 1 << 0,

        /// <summary>
        ///     The error indicating that the voltage level is invalid for a reel
        /// </summary>
        [ErrorGuid("{DF57B5CC-EF18-4B77-A925-A3BEB0A07DB0}", DisplayableMessageClassification.HardError)]
        LowVoltage = 1 << 1,

        /// <summary>
        ///     The error when a reel has stalled during a spin
        /// </summary>
        [ErrorGuid("{CB863818-696E-4443-9DCB-1039151CB5FE}", DisplayableMessageClassification.HardError)]
        ReelStall = 1 << 2,

        /// <summary>
        ///     The error used to represent when a reel has been moved from its idle position
        /// </summary>
        [ErrorGuid("{C188AE59-D9F0-4765-8A38-A7D85F77C301}", DisplayableMessageClassification.HardError)]
        ReelTamper = 1 << 3,

        /// <summary>
        ///     The error used to represent when a reel has encountered an optic sequence error.
        /// </summary>
        [ErrorGuid("{18758D98-7EE2-4743-8264-68A8C67AD6B2}", DisplayableMessageClassification.HardError)]
        ReelOpticSequenceError = 1 << 4,
        /// <summary>
        ///     The error used to represent when a reel has stopped at an unknown stop.
        /// </summary>
        [ErrorGuid("{9EC975FB-93EF-4A96-95EE-07804A67CA74}", DisplayableMessageClassification.HardError)]
        IdleUnknown = 1 << 5,

        /// <summary>
        ///     The error used to represent when a reel is required to stop at an unknown stop.
        /// </summary>
        [ErrorGuid("{0B11A291-7CB6-4A8C-A89F-AD98B159F8EE}", DisplayableMessageClassification.HardError)]
        UnknownStop = 1 << 6,
    }

    /// <summary>
    ///     States for the reel controller
    /// </summary>
    public enum ReelControllerState
    {
        /// <summary>
        ///     The uninitialized state
        /// </summary>
        Uninitialized = 0,

        /// <summary>
        ///     The inspecting state
        /// </summary>
        Inspecting,

        /// <summary>
        ///     The state for idling at an unknown stop
        /// </summary>
        IdleUnknown,

        /// <summary>
        ///     The state for idling at a known stop
        /// </summary>
        IdleAtStops,

        /// <summary>
        ///     The state for homing the reels
        /// </summary>
        Homing,

        /// <summary>
        ///     The state for spinning the reels
        /// </summary>
        Spinning,

        /// <summary>
        ///     The state for tilted reels (slow spinning)
        /// </summary>
        Tilted,

        /// <summary>
        ///     The disabled state
        /// </summary>
        Disabled,

        /// <summary>
        ///     The disconnected state
        /// </summary>
        Disconnected
    }

    /// <summary>
    ///    The states for the reels that can occur
    /// </summary>
    public enum ReelLogicalState
    {
        /// <summary>
        ///     The state used for when the reel is disconnected
        /// </summary>
        Disconnected = 0,

        /// <summary>
        ///     The state used when the reels are idle at an unknown stop
        /// </summary>
        IdleUnknown,

        /// <summary>
        ///     The state used for when the reel is idle at a known stop
        /// </summary>
        IdleAtStop,

        /// <summary>
        ///     The state used for when the reel is spinning
        /// </summary>
        Spinning,

        /// <summary>
        ///      The state used for when the reel is spinning freely in a forward direction
        /// </summary>
        SpinningForward,

        /// <summary>
        ///     The state used for when the reel is spinning at a constant velocity
        /// </summary>
        SpinningConstant,

        /// <summary>
        ///     The state used for when the reel is spinning freely in a backwards direction
        /// </summary>
        SpinningBackwards,

        /// <summary>
        ///     The state for when the reels are being homed to known stop locations.
        /// </summary>
        Homing,

        /// <summary>
        ///     The state used for when the reel is stopped
        /// </summary>
        Stopping,

        /// <summary>
        ///     The state used for when the reels are tilted (slow spinning)
        /// </summary>
        Tilted,

        /// <summary>
        ///      The state used for when the reel is accelerating in a forward direction
        /// </summary>
        Accelerating,

        /// <summary>
        ///      The state used for when the reel is decelerating in a forward direction
        /// </summary>
        Decelerating
    }

    /// <summary>
    ///     The triggers for handling state changes for the reel controller
    /// </summary>
    public enum ReelControllerTrigger
    {
        /// <summary>
        ///     The trigger for disabling the reel controller
        /// </summary>
        Disable,

        /// <summary>
        ///     The trigger for enabling the reel controller
        /// </summary>
        Enable,

        /// <summary>
        ///     The trigger for inspecting
        /// </summary>
        Inspecting,

        /// <summary>
        ///     The trigger for inspection failed
        /// </summary>
        InspectionFailed,

        /// <summary>
        ///     The trigger when initialized
        /// </summary>
        Initialized,

        /// <summary>
        ///     The trigger for disconnection
        /// </summary>
        Disconnected,

        /// <summary>
        ///     The trigger for connection
        /// </summary>
        Connected,

        /// <summary>
        ///     The home reels trigger
        /// </summary>
        HomeReels,

        /// <summary>
        ///     The spin reel in a forward direction trigger
        /// </summary>
        SpinReel,

        /// <summary>
        ///     The spin reel in a backwards direction trigger
        /// </summary>
        SpinReelBackwards,

        /// <summary>
        ///     The spin reel constant trigger
        /// </summary>
        SpinConstant,

        /// <summary>
        ///     The tilt reels trigger
        /// </summary>
        TiltReels,

        /// <summary>
        ///     The reel stopped trigger
        /// </summary>
        ReelStopped,

        /// <summary>
        ///     The accelerate trigger
        /// </summary>
        Accelerate,

        /// <summary>
        ///     The decelerate trigger
        /// </summary>
        Decelerate
    }

    /// <summary>
    ///     The direction for the reels to spin
    /// </summary>
    public enum SpinDirection
    {
        /// <summary>
        ///     This is used to indicate the reels should spin in the forward direction
        /// </summary>
        Forward = 0,

        /// <summary>
        ///     This is used to indicate the reels should spin in the reverse direction
        /// </summary>
        Backwards
    }

    /// <summary>
    ///     Describes the velocity of the spin of the reels
    /// </summary>
    public enum SpinVelocity
    {
        /// <summary>
        ///     Not specified
        /// </summary>
        None,

        /// <summary>
        ///     Constant velocity
        /// </summary>
        Constant,

        /// <summary>
        ///     Accelerating velocity
        /// </summary>
        Accelerating,

        /// <summary>
        ///     Decelerating velocity
        /// </summary>
        Decelerating
    }

    /// <summary>
    ///     Indicators for an animation's state 
    /// </summary>
    public enum AnimationState : byte
    {
        /// <summary>Indicates the animation was prepared</summary>
        Prepared,

        /// <summary>Indicates the animation was stopped</summary>
        Stopped,

        /// <summary>Indicates the animation was started</summary>
        Started,

        /// <summary>Indicates the animation was removed from the playing queue</summary>
        Removed,

        /// <summary>Indicates all animations were cleared</summary>
        AllAnimationsCleared
    }

    /// <summary>
    ///     Animation file type
    /// </summary>
    public enum AnimationType
    {
        /// <summary>Platform owned light show animations</summary>
        PlatformLightShow,

        /// <summary>Platform stepper curve animations</summary>
        PlatformStepperCurve,

        /// <summary>Game owned light show animations</summary>
        GameLightShow,

        /// <summary>Game stepper curve animations</summary>
        GameStepperCurve
    }

    /// <summary>
    ///     The type of synchronize
    /// </summary>
    public enum SynchronizeType
    {
        /// <summary>
        ///     Denotes that the regular synchronize command will be used
        /// </summary>
        Regular,

        /// <summary>
        ///     Denotes that the enhanced synchronize command will be used
        /// </summary>
        Enhanced
    }

    /// <summary>
    ///     The type of synchronize status
    /// </summary>
    public enum SynchronizeStatus
    {
        /// <summary>
        ///     Denotes that reel synchronization has started
        /// </summary>
        Started,

        /// <summary>
        ///     Denotes that synchronization has completed
        /// </summary>
        Complete
    }

    /// <summary>
    ///     The stepper rule type
    /// </summary>
    public enum StepperRuleType
    {
        /// <summary>
        ///     Denotes an anticipation rule
        /// </summary>
        AnticipationRule,

        /// <summary>
        ///     Denotes a follow rule
        /// </summary>
        FollowRule
    }

    /// <summary>
    ///     The animation prepared status
    /// </summary>
    public enum AnimationPreparedStatus
    {
        /// <summary>
        ///     Preparation status is unknown
        /// </summary>
        Unknown,

        /// <summary>
        ///     The animation was prepared successfully.
        /// </summary>
        Prepared,

        /// <summary>
        ///     The show does not exist.
        /// </summary>
        DoesNotExist,

        /// <summary>
        ///     The file is corrupt.
        /// </summary>
        FileCorrupt,

        /// <summary>
        ///     The animation queue is full.
        /// </summary>
        QueueFull,

        /// <summary>
        ///     The animation is incompatible with the current controller state.
        /// </summary>
        IncompatibleState
    }

    /// <summary>
    ///     The animation queue type
    /// </summary>
    public enum AnimationQueueType
    {
        /// <summary>
        ///     Queue location is unknown
        /// </summary>
        Unknown,

        /// <summary>
        ///     Removed from playing queue.
        /// </summary>
        PlayingQueue,

        /// <summary>
        ///     Removed from waiting queue.
        /// </summary>
        WaitingQueue,

        /// <summary>
        ///     Removed from playing and waiting queue.
        /// </summary>
        PlayAndWaitQueues,

        /// <summary>
        ///     Removed from play because the animation ended.
        /// </summary>
        AnimationEnded,

        /// <summary>
        ///     Not in the animation queues.
        /// </summary>
        NotInQueues
    }
}
