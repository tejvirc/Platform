namespace Aristocrat.Bingo.Client.Messages
{
    using System;

    /// <summary>
    ///     The message for reporting events to the server
    /// </summary>
    [Serializable]
    public class ReportEventMessage : IMessage
    {
        /// <summary>
        ///     Creates an instance of <see cref="ReportEventMessage"/>
        /// </summary>
        /// <param name="machineSerial">The machine serial for this event</param>
        /// <param name="timeStamp">The timestamp for this event</param>
        /// <param name="eventId">The event ID for this event</param>
        /// <param name="eventType">The event type for this event</param>
        public ReportEventMessage(
            string machineSerial,
            DateTime timeStamp,
            long eventId,
            int eventType)
        {
            MachineSerial = machineSerial;
            TimeStamp = timeStamp;
            EventId = eventId;
            EventType = eventType;
        }

        /// <summary>
        ///     Gets the machine serial
        /// </summary>
        public string MachineSerial { get; set; }

        /// <summary>
        ///     Gets the timestamp for this event
        /// </summary>
        public DateTime TimeStamp { get; }

        /// <summary>
        ///     Gets the event ID for this event
        /// </summary>
        public long EventId { get; }

        /// <summary>
        ///     Gets the event type for this event
        /// </summary>
        public int EventType { get; }
    }
}