namespace Aristocrat.Bingo.Client.Messages
{
    public class ConfigurationMessage : IMessage
    {
        public ConfigurationMessage(string machineSerial, string gameTitles)
        {
            MachineSerial = machineSerial;
            GameTitles = gameTitles;
        }

        public string MachineSerial { get; }

        public string GameTitles { get; }
    }
}
