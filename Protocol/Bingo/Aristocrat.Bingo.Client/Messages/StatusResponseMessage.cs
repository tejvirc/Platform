namespace Aristocrat.Bingo.Client.Messages
{
    public class StatusResponseMessage : IMessage
    {
        public StatusResponseMessage(string machineSerial)
        {
            MachineSerial = machineSerial;
            EgmStatusFlags = 0;
        }

        public string MachineSerial { get; }

        public long CashPlayedMeterValue { get; set; }

        public long CashWonMeterValue { get; set; }

        public long CashInMeterValue { get; set; }

        public long CashOutMeterValue { get; set; }

        public int EgmStatusFlags { get; set; }
    }
}