namespace Aristocrat.Bingo.Client.Messages
{
    using ProtoBuf;
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     The message for reporting events to the server
    /// </summary>
    [ProtoContract]
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
        /// Parameterless constructor used while deseriliazing 
        /// </summary>
        public ReportEventMessage()
        {
        }

        [ProtoMember(1)]
        public string MachineSerial { get; set; }

        /// <summary>
        ///     Gets the timestamp for this event
        /// </summary>
        [ProtoMember(2)]
        public DateTime TimeStamp { get; }

        /// <summary>
        ///     Gets the event ID for this event
        /// </summary>
        [ProtoMember(3)]
        public long EventId { get; }

        /// <summary>
        ///     Gets the event type for this event
        /// </summary>
        [ProtoMember(4)]
        public int EventType { get; }
    }
}