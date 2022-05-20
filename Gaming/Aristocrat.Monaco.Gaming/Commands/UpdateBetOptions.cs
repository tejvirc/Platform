namespace Aristocrat.Monaco.Gaming.Commands
{
    public class UpdateBetOptions
    {
        public UpdateBetOptions(
            long wager,
            int betMultiplier,
            int lineCost,
            int numberLines,
            int ante,
            int betLinePresetId)
        {
            Wager = wager;
            BetMultiplier = betMultiplier;
            LineCost = lineCost;
            NumberLines = numberLines;
            Ante = ante;
            BetLinePresetId = betLinePresetId;
        }

        public long Wager { get; }

        public int BetMultiplier { get; }

        public int LineCost { get; }

        public int NumberLines { get; }

        public int Ante { get; }

        public int BetLinePresetId { get; }
    }
}