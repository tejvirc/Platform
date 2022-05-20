namespace Aristocrat.Monaco.Application.Contracts.Input
{
    /// <summary>
    ///     Provides an API for calibrating the Touch Screens. After calibration session is complete, the
    ///     <see cref="TouchCalibrationCompletedEvent"/> will be sent. Subscribe to this event to be notified when calibration
    ///     has completed.
    /// </summary>
    public interface ITouchCalibration
    {
        /// <summary>
        ///     Starts the calibration session.
        /// </summary>
        /// <returns>true if calibration has begun, otherwise false.</returns>
        bool BeginCalibration();

        /// <summary>
        ///     Proceeds to calibrate the next touch screen device.
        /// </summary>
        void CalibrateNextDevice();

        /// <summary>
        ///     Ends calibration early and rolls back any changes made during the session.
        /// </summary>
        void AbortCalibration();

        /// <summary>
        ///     True when a calibration session is underway, otherwise false.
        /// </summary>
        bool IsCalibrating { get; }
    }
}

