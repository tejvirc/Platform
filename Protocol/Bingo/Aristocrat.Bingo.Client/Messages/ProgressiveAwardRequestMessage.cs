namespace Aristocrat.Bingo.Client.Messages
{
    public class ProgressiveAwardRequestMessage : IMessage
    {
        public ProgressiveAwardRequestMessage(
            string machineSerial,
            int progressiveAwardId,
            int levelId,
            long amount,
            bool pending)
        {
            MachineSerial = machineSerial;
            ProgressiveAwardId = progressiveAwardId;
            LevelId = levelId;
            Amount = amount;
            Pending = pending;
        }

        public string MachineSerial { get; }

        public int ProgressiveAwardId { get; }

        public int LevelId { get; }

        public long Amount { get; }

        public bool Pending { get; }
    }
}