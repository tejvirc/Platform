namespace Aristocrat.Monaco.Hardware.Contracts.Reel
{
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
}