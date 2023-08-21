namespace Aristocrat.Monaco.Application.Contracts.Input
{
    using Aristocrat.Monaco.Hardware.Contracts.Touch;

    /// <summary>
    ///     Provides an API for calibrating the serial touch screens.  After calibration session is complete, the
    ///     <see cref="SerialTouchCalibrationCompletedEvent" /> will be sent.  Subscribe to this event to be notified when calibration
    ///     has ended.
    /// </summary>
    public interface ISerialTouchCalibration
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