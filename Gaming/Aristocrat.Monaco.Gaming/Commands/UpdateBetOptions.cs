namespace Aristocrat.Monaco.Gaming.Commands
{
    public class UpdateBetOptions
    {
        public UpdateBetOptions(
            long wager,
            long stake,
            int betMultiplier,
            int lineCost,
            int numberLines,
            int ante,
            int betLinePresetId)
        {
            Wager = wager;
            Stake = stake;
            BetMultiplier = betMultiplier;
            LineCost = lineCost;
            NumberLines = numberLines;
            Ante = ante;
            BetLinePresetId = betLinePresetId;
        }

        public long Wager { get; }

        public long Stake { get; }

        public int BetMultiplier { get; }

        public int LineCost { get; }

        public int NumberLines { get; }

        public int Ante { get; }

        public int BetLinePresetId { get; }
    }
}