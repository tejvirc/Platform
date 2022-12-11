namespace Aristocrat.Bingo.Client.Messages
{
    using ProtoBuf;
    using System;
    using System.Runtime.Serialization;

    [ProtoContract]
    public class ReportEventMessage : IMessage
    {
        public ReportEventMessage(
            string machineSerial,
            DateTime timeStamp,
            int eventId,
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

        [ProtoMember(2)]
        public DateTime TimeStamp { get; }

        [ProtoMember(3)]
        public int EventId { get; }

        [ProtoMember(4)]
        public int EventType { get; }
    }
}