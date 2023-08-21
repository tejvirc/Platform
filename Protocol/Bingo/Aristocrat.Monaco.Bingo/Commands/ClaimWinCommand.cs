namespace Aristocrat.Monaco.Bingo.Commands
{
    public class ClaimWinCommand
    {
        public ClaimWinCommand(
            string machineSerial,
            long gameSerial,
            uint cardSerial,
            long betAmount)
        {
            MachineSerial = machineSerial;
            GameSerial = gameSerial;
            CardSerial = cardSerial;
            BetAmount = betAmount;
        }

        public string MachineSerial { get; }

        public long GameSerial { get; }

        public uint CardSerial { get; }

        public long BetAmount { get; }
    }
}