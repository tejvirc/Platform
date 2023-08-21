namespace Aristocrat.Monaco.Application.Contracts.TiltLogger
{
    using System;

    /// <summary>
    ///     Encapsulates the data for an event signaling that the tilt log has
    ///     been appended.
    /// </summary>
    public class TiltLogAppendedEventArgs : EventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TiltLogAppendedEventArgs" /> class.
        /// </summary>
        /// <param name="message">The log message.</param>
        /// <param name="tiltType">The tilt type.</param>
        public TiltLogAppendedEventArgs(EventDescription message, Type tiltType)
        {
            Message = message;
            TiltType = tiltType;
        }

        /// <summary>
        ///     Gets the message that was just appended to the tilt log.
        /// </summary>
        public EventDescription Message { get; }

        /// <summary>
        ///     Gets the type of event that caused the tilt.
        /// </summary>
        public Type TiltType { get; }
    }
}