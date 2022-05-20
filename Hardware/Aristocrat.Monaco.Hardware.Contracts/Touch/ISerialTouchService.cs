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
        ///     Sends a calibrate extended command to the connected serial touch device
        /// </summary>
        void SendCalibrateExtendedCommand();

        /// <summary>
        ///     Sends a diagnostic command to the connected serial touch device
        /// </summary>
        void SendDiagnosticCommand();

        /// <summary>
        ///     Sends a name command to the connected serial touch device
        /// </summary>
        void SendNameCommand();

        /// <summary>
        ///     Sends a null command to the connected serial touch device
        /// </summary>
        void SendNullCommand();

        /// <summary>
        ///     Sends a output identity command to the connected serial touch device
        /// </summary>
        void SendOutputIdentityCommand();

        /// <summary>
        ///     Sends a reset command to the connected serial touch device
        /// </summary>
        /// <param name="calibrating">Indicates whether or not we are calibrating</param>
        void SendResetCommand(bool calibrating = false);

        /// <summary>
        ///     Sends a restore defaults command to the connected serial touch device
        /// </summary>
        /// <param name="calibrating">Indicates whether or not we are calibrating</param>
        void SendRestoreDefaultsCommand(bool calibrating = false);

        /// <summary>
        ///     Gets or sets the status of the connected serial touch device
        /// </summary>
        string Status { get; set; }
    }
}