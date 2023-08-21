namespace Aristocrat.Monaco.Application.Contracts
{
    using System;

    /// <summary>
    ///     Encapsulates the data for an event signaling that the door monitor log has
    ///     been appended.
    /// </summary>
    public class DoorMonitorAppendedEventArgs : EventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DoorMonitorAppendedEventArgs" /> class.
        /// </summary>
        /// <param name="message">The log message.</param>
        public DoorMonitorAppendedEventArgs(DoorMessage message)
        {
            DoorId = message.DoorId;
            Time = message.Time;
            IsOpen = message.IsOpen;
            ValidationPassed = message.ValidationPassed;
        }

        /// <summary>
        ///     Gets the door id of the message that was just appended to the door monitor log.
        /// </summary>
        public int DoorId { get; }

        /// <summary>
        ///     Gets the time of the message that was appended to the door monitor log.
        /// </summary>
        public DateTime Time { get; }

        /// <summary>
        ///     Gets a value indicating whether or not the door was opened.
        /// </summary>
        public bool IsOpen { get; }

        /// <summary>
        ///     Gets a value indicating whether or not the event passed validation.
        /// </summary>
        public bool ValidationPassed { get; }
    }
}