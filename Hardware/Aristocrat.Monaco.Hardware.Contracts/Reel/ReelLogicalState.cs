namespace Aristocrat.Monaco.Hardware.Contracts.Reel
{
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
}