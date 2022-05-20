namespace Aristocrat.Monaco.Hardware.Contracts.Touch
{
    using Kernel;

    /// <summary>
    ///     This event is sent when serial touch calibration is complete or in error.
    /// </summary>
    /// <seealso cref="BaseEvent" />
    public class SerialTouchCalibrationCompletedEvent : BaseEvent
    {
        /// <inheritdoc />
        public SerialTouchCalibrationCompletedEvent(bool success, string status)
        {
            Success = success;
            Status = status;
        }

        /// <summary>
        ///     Gets a value indicating whether this <see cref="SerialTouchCalibrationCompletedEvent" /> is successful.
        /// </summary>
        public bool Success { get; }

        /// <summary>Gets the status message associated with a calibration attempt.</summary>
        public string Status { get; }
    }
}