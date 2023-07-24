namespace Aristocrat.Bingo.Client.Messages
{
    public class ProgressiveClaimRequestMessage : IMessage
    {
        public ProgressiveClaimRequestMessage(string machineSerial, long progressiveLevelId, long progressiveWinAmount)
        {
            MachineSerial = machineSerial;
            ProgressiveLevelId = progressiveLevelId;
            ProgressiveWinAmount = progressiveWinAmount;
        }

        public string MachineSerial { get; }

        public long ProgressiveLevelId { get; }

        public long ProgressiveWinAmount { get; }
    }
}