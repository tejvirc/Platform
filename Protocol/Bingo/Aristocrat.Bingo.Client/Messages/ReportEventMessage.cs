namespace Aristocrat.Bingo.Client.Messages
{
    using System;

    [Serializable]
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

        public string MachineSerial { get; set; }

        public DateTime TimeStamp { get; }

        public int EventId { get; }

        public int EventType { get; }
    }
}