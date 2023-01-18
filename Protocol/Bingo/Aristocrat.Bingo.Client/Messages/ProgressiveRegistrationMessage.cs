namespace Aristocrat.Bingo.Client.Messages
{
    public class ProgressiveRegistrationMessage : IMessage
    {
        public ProgressiveRegistrationMessage(string machineSerial, int gameTitleId)
        {
            MachineSerial = machineSerial;
            GameTitleId = gameTitleId;
        }

        public string MachineSerial { get; }

        public int GameTitleId { get; }
    }
}