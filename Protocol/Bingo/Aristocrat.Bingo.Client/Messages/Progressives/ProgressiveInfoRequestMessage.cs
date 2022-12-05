namespace Aristocrat.Bingo.Client.Messages.Progressives
{
    public class ProgressiveInfoRequestMessage : IMessage
    {
        public ProgressiveInfoRequestMessage(
            string machineSerial,
            int gameTitleId)
        {
            MachineSerial = machineSerial;
            GameTitleId = gameTitleId;
        }

        public string MachineSerial { get; }

        public int GameTitleId { get; }
    }
}