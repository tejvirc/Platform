﻿namespace Aristocrat.Monaco.Application.Contracts.Input
{
    using Kernel;

    /// <summary>
    ///     This event is sent after a touch calibration session has concluded.
    /// </summary>
    /// <seealso cref="BaseEvent" />
    public class TouchCalibrationCompletedEvent: BaseEvent
    {
        /// <inheritdoc />
        public TouchCalibrationCompletedEvent(bool success, string error)
        {
            Success = success;
            Error = error;
        }

        /// <summary>Gets a value indicating whether or not touch calibration completed successfully.</summary>
        public bool Success { get; }

        /// <summary>Gets the error message associated with a failed calibration attempt.</summary>
        public string Error { get; }
    }
}