namespace Aristocrat.Monaco.Bingo.Commands
{
    public class RequestPlayCommand
    {
        public RequestPlayCommand(
            string machineSerial,
            long betAmount,
            int activeDenomination,
            int betLinePresetId,
            int lineBet,
            int lines,
            long ante,
            string activeGameTitles)
        {
            MachineSerial = machineSerial;
            BetAmount = betAmount;
            ActiveDenomination = activeDenomination;
            BetLinePresetId = betLinePresetId;
            LineBet = lineBet;
            Lines = lines;
            Ante = ante;
            ActiveGameTitles = activeGameTitles;
        }

        public string MachineSerial { get; }

        public long BetAmount { get; }

        public int ActiveDenomination { get; }

        public int BetLinePresetId { get; }

        public int LineBet { get; }

        public int Lines { get; }

        public long Ante { get; }

        public string ActiveGameTitles { get; }
    }
}