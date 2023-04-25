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
        ///     Gets whether or not data has been received from the serial touch device
        /// </summary>
        bool HasReceivedData { get; }

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
        ///     Initializes touch injection
        /// </summary>
        /// <returns>True on success, otherwise false</returns>
        bool InitializeTouchInjection();

        /// <summary>
        ///     Starts the serial touch calibration
        /// </summary>
        void StartCalibration();

        /// <summary>
        ///     Cancels any active serial touch calibration
        /// </summary>
        void CancelCalibration();
    }
}