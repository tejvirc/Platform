namespace Aristocrat.Monaco.Hardware.Contracts.Touch
{
    /// <summary>
    ///     Enumeration of calibration states for setting crosshair colors
    /// </summary>
    public enum CalibrationCrosshairColors
    {
        /// <summary>
        ///     Used to set the calibration target crosshair color transparent when calibration inactive
        /// </summary>
        Inactive,

        /// <summary>
        ///     Used to set the calibration target crosshair color black when calibration active
        /// </summary>
        Active,

        /// <summary>
        ///     Used to set the calibration target crosshair color green when calibration is acknowledged
        /// </summary>
        Acknowledged,

        /// <summary>
        ///     Used to set the calibration target crosshair color red when calibration is in error
        /// </summary>
        Error
    }

    /// <summary>
    ///     Provides a mechanism to interact with the serial touch service
    /// </summary>
    public interface ISerialTouchService
    {
        /// <summary>
        ///     Gets whether or not the serial touch device is initialized
        /// </summary>
        bool Initialized { get; }

        /// <summary>
        ///     Gets whether or not the serial touch device is disconnected
        /// </summary>
        bool IsDisconnected { get; }

        /// <summary>
        ///     Gets whether or not the TabletInputService startup type is Manual.
        /// </summary>
        /// <remarks>For cabinet configurations without HID/USB based touch controllers (IE. LS), the
        /// start-up type of the TabletInputService is responsible for the OS opening/closing the on-screen
        /// keyboard.  If set to Manual by the OS, we need to explicitly open/close the on-screen keyboard
        /// whenever a TextBox controls gets/loses focus.  If set to Automatic, we can skip explicitly opening
        /// the on-screen keyboard as the OS should automatically do this.</remarks>
        bool IsManualTabletInputService { get; }

        /// <summary>
        ///     Gets the model of the connected serial touch device
        /// </summary>
        string Model { get; }

        /// <summary>
        ///     Gets the output identity of the connected serial touch device
        /// </summary>
        string OutputIdentity { get; }

        /// <summary>
        ///     Gets whether or not the serial touch device is pending calibration
        /// </summary>
        bool PendingCalibration { get; }

        /// <summary>
        ///     Reconnects the serial port controller
        /// </summary>
        /// <param name="calibrating">Indicates whether or not we are calibrating</param>
        void Reconnect(bool calibrating = false);

        /// <summary>
        ///     Sends a reset command to the connected serial touch device
        /// </summary>
        /// <param name="calibrating">Indicates whether or not we are calibrating</param>
        void SendResetCommand(bool calibrating = false);

        /// <summary>
        ///     Gets or sets the status of the connected serial touch device
        /// </summary>
        string Status { get; set; }
    }
}