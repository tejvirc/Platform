namespace Aristocrat.Monaco.Hardware.Contracts.Touch
{
    /// <summary>
    ///     States for serial touch
    /// </summary>
    public enum SerialTouchState
    {
        /// <summary>
        ///     The uninitialized state
        /// </summary>
        Uninitialized = 0,

        /// <summary>
        ///     The Initialize state
        /// </summary>
        Initialize,

        /// <summary>
        ///     The state used to query the controller and wait for a response
        /// </summary>
        Null,

        /// <summary>
        ///     The state used to request the name of the serial touch device
        /// </summary>
        Name,

        /// <summary>
        ///     The state used to request the output identity from the serial touch device
        /// </summary>
        OutputIdentity,

        /// <summary>
        ///     The state used to instruct the serial touch device to reset
        /// </summary>
        Reset,

        /// <summary>
        ///     The state used to instruct the serial device to restore default settings
        /// </summary>
        RestoreDefaults,

        /// <summary>
        ///     The state used to initiate calibration of the serial touch device
        /// </summary>
        CalibrateExtended,

        /// <summary>
        ///     The state used for calibrating the lower left target
        /// </summary>
        LowerLeftTarget,

        /// <summary>
        ///     The state used for calibrating the upper right target
        /// </summary>
        UpperRightTarget,

        /// <summary>
        ///     The state used for interpreting raw touch data from thew serial touch device
        /// </summary>
        InterpretTouch,

        /// <summary>
        ///     The error state
        /// </summary>
        Error
    }
}