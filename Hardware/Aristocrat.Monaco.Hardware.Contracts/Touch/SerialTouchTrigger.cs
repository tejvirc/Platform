﻿namespace Aristocrat.Monaco.Hardware.Contracts.Touch
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
        ///     The trigger when initialized
        /// </summary>
        Initialized,

        /// <summary>
        ///     The trigger for interpret touch
        /// </summary>
        InterpretTouch,

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
        ///     The trigger for diagnostic
        /// </summary>
        Diagnostic,

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