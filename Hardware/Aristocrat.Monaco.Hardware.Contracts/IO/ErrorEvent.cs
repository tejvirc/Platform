namespace Aristocrat.Monaco.Hardware.Contracts.IO
{
    using System;
    using System.Globalization;
    using Kernel;

    /// <summary>Valid IO error event ID enumerations.</summary>
    public enum ErrorEventId
    {
        /// <summary>Indicates IO error event ID none.</summary>
        None = 0,

        /// <summary>Indicates IO error event ID read.</summary>
        Read,

        /// <summary>Indicates IO error event ID write.</summary>
        Write,

        /// <summary>Indicates IO error event ID InvalidHandle.</summary>
        InvalidHandle,

        /// <summary>Indicates IO error event ID ReadBoardInfoFailure.</summary>
        ReadBoardInfoFailure,

        /// <summary>Indicates IO error event ID BatteryStatusFailure.</summary>
        BatteryStatusFailure,

        /// <summary>Indicates IO error event ID InvalidInputRequest.</summary>
        InvalidInputRequest,

        /// <summary>Indicates IO error event ID InvalidOutputRequest.</summary>
        InvalidOutputRequest,

        /// <summary>Indicates IO error event ID WatchdogEnableFailure.</summary>
        WatchdogEnableFailure,

        /// <summary>Indicates IO error event ID WatchdogDisableFailure.</summary>
        WatchdogDisableFailure,

        /// <summary>Indicates IO error event ID WatchdogResetFailure.</summary>
        WatchdogResetFailure,

        /// <summary>Indicates IO failed input.</summary>
        InputFailure,

        /// <summary>Indicates IO failed output.</summary>
        OutputFailure
    }

    /// <summary>Definition of the ErrorEvent class.</summary>
    [Serializable]
    public class ErrorEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ErrorEvent" /> class.
        /// </summary>
        public ErrorEvent()
        {
            Id = ErrorEventId.None;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ErrorEvent" /> class.
        /// </summary>
        /// <param name="id">ID of the error event.</param>
        public ErrorEvent(ErrorEventId id)
        {
            Id = id;
        }

        /// <summary>Gets the ID of the error event.</summary>
        public ErrorEventId Id { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0} [Id={1}]",
                GetType().Name,
                Id);
        }
    }
}