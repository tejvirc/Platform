namespace Aristocrat.Bingo.Client.Messages
{
    using System.Collections.Generic;

    public class ProgressiveRegistrationMessage : IMessage
    {
        public ProgressiveRegistrationMessage(string machineSerial, int gameTitleId, IReadOnlyCollection<ProgressiveGameRegistrationData> games)
        {
            MachineSerial = machineSerial;
            GameTitleId = gameTitleId;
            Games = games;
        }

        public string MachineSerial { get; }

        public int GameTitleId { get; }

        public IReadOnlyCollection<ProgressiveGameRegistrationData> Games { get; set; }
    }
}