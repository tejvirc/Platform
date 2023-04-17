namespace Aristocrat.Bingo.Client.Messages
{
    public class RegistrationMessage : IMessage
    {
        public RegistrationMessage(
            string machineSerial,
            string machineNumber,
            string platformVersion,
            string machineConnectionId)
        {
            MachineSerial = machineSerial;
            MachineNumber = machineNumber;
            PlatformVersion = platformVersion;
            MachineConnectionId = machineConnectionId;
        }

        public string MachineSerial { get; }

        public string MachineNumber { get; }

        public string PlatformVersion { get; }

        public string MachineConnectionId { get; }
    }
}