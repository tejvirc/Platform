namespace Aristocrat.Bingo.Client.Messages
{
    public class RegistrationMessage : IMessage
    {
        public RegistrationMessage(string machineSerial, string machineNumber, string platformVersion)
        {
            MachineSerial = machineSerial;
            MachineNumber = machineNumber;
            PlatformVersion = platformVersion;
        }

        public string MachineSerial { get; }

        public string MachineNumber { get; }

        public string PlatformVersion { get; }
    }
}