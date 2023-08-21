namespace Aristocrat.Bingo.Client.Messages.GamePlay
{
    public class RequestClaimWinMessage : IMessage
    {
        public RequestClaimWinMessage(
            string machineSerial,
            long gameSerial,
            uint cardSerial)
        {
            MachineSerial = machineSerial;
            GameSerial = gameSerial;
            CardSerial = cardSerial;
        }

        public string MachineSerial { get; }

        public long GameSerial { get; }

        public uint CardSerial { get; }
    }
}