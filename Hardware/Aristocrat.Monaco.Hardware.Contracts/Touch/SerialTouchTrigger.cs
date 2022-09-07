namespace Aristocrat.Monaco.Hardware.Contracts.Touch
{
    /// <summary>
    ///     The triggers for handling state changes for serial touch
    /// </summary>
    public enum SerialTouchTrigger
    {
        /// <summary>
        ///     The trigger when uninitialized
        /// </summary>
        Uninitialized,

        /// <summary>
        ///     The trigger for initializing the touch input
        /// </summary>
        Initialize,

        /// <summary>
        ///     The trigger for null
        /// </summary>
        Null,

        /// <summary>
        ///     The trigger for interpret touch
        /// </summary>
        InterpretTouch,

        /// <summary>
        ///     The trigger for handling when null completes
        /// </summary>
        NullCompleted,

        /// <summary>
        ///     The trigger for name
        /// </summary>
        Name,

        /// <summary>
        ///     The trigger for output identity
        /// </summary>
        OutputIdentity,

        /// <summary>
        ///     The trigger for restore defaults
        /// </summary>
        RestoreDefaults,

        /// <summary>
        ///     The trigger for reset
        /// </summary>
        Reset,

        /// <summary>
        ///     The trigger for calibrate extended
        /// </summary>
        CalibrateExtended,

        /// <summary>
        ///     The trigger for lower left target
        /// </summary>
        LowerLeftTarget,

        /// <summary>
        ///     The trigger for upper right target
        /// </summary>
        UpperRightTarget,

        /// <summary>
        ///     The trigger for error
        /// </summary>
        Error
    }
}