namespace Aristocrat.Monaco.Hardware.Contracts.Touch
{
    using Kernel;

    /// <summary>
    ///     This event is sent whenever the serial touch calibration status is updated.
    /// </summary>
    /// <seealso cref="BaseEvent" />
    public class SerialTouchCalibrationStatusEvent : BaseEvent
    {
        /// <inheritdoc />
        public SerialTouchCalibrationStatusEvent(string error, string resourceKey, CalibrationCrosshairColors crosshairColorLowerLeft, CalibrationCrosshairColors crosshairColorUpperRight)
        {
            Error = error;
            ResourceKey = resourceKey;
            CrosshairColorLowerLeft = crosshairColorLowerLeft;
            CrosshairColorUpperRight = crosshairColorUpperRight;
        }

        /// <summary>Gets the error code to display.</summary>
        public string Error { get; }

        /// <summary>Gets the resource key for the localized message.</summary>
        public string ResourceKey { get; }

        /// <summary>
        ///     Gets the lower left crosshair color.
        /// </summary>
        public CalibrationCrosshairColors CrosshairColorLowerLeft { get; }

        /// <summary>
        ///     Gets the upper right crosshair color.
        /// </summary>
        public CalibrationCrosshairColors CrosshairColorUpperRight { get; }
    }
}