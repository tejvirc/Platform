namespace Aristocrat.Bingo.Client.Messages
{
    public class StatusMessage : IMessage
    {
        public StatusMessage(
            string machineSerial,
            long cashPlayedMeterValue,
            long cashWonMeterValue,
            long cashInMeterValue,
            long cashOutMeterValue,
            int egmStatusFlags)
        {
            MachineSerial = machineSerial;
            CashPlayedMeterValue = cashPlayedMeterValue;
            CashWonMeterValue = cashWonMeterValue;
            CashInMeterValue = cashInMeterValue;
            CashOutMeterValue = cashOutMeterValue;
            EgmStatusFlags = egmStatusFlags;
        }

        public string MachineSerial { get; }

        public long CashPlayedMeterValue { get; }

        public long CashWonMeterValue { get; }

        public long CashInMeterValue { get; }

        public long CashOutMeterValue { get; }

        public int EgmStatusFlags { get; }
    }
}