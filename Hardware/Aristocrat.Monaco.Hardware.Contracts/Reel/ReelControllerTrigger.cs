namespace Aristocrat.Monaco.Hardware.Contracts.Reel
{
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
        ///     The tilt reels trigger
        /// </summary>
        TiltReels,

        /// <summary>
        ///     The reel stopped trigger
        /// </summary>
        ReelStopped,

        /// <summary>
        ///     The accelerate reel in a forward direction trigger
        /// </summary>
        SpinForwardAccelerate,

        /// <summary>
        ///     The decelerate reel in a forward direction trigger
        /// </summary>
        SpinForwardDecelerate,

        /// <summary>
        ///     The accelerate reel in a backwards direction trigger
        /// </summary>
        SpinBackwardAccelerate,

        /// <summary>
        ///     The decelerate reel in a backwards direction trigger
        /// </summary>
        SpinBackwardDecelerate
    }
}