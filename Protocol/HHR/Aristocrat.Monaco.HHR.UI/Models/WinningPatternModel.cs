namespace Aristocrat.Monaco.Hhr.UI.Models
{
    public class WinningPatternModel
    {
        public WinningPatternModel(byte[] patterns,
            string wager,
            uint raceSet,
            bool includesProgressiveResetValues,
            double extraWinnings,
            long guaranteedCredits)
        {
            Patterns = patterns;
            Wager = wager;
            RaceSet = raceSet;
            IncludesProgressiveResetValues = includesProgressiveResetValues;
            ExtraWinnings = extraWinnings;
            GuaranteedCredits = guaranteedCredits;
        }

        public WinningPatternModel()
        {
            Patterns = new byte[5];
            Wager = "";
            RaceSet = 0;
            IncludesProgressiveResetValues = false;
            ExtraWinnings = 0;
            GuaranteedCredits = 0;
        }


        /// <summary>
        ///     Collection of Winning Patterns
        /// </summary>
        public byte[] Patterns { get; set; }

        /// <summary>
        ///     Wager
        /// </summary>
        public string Wager { get; set; }

        /// <summary>
        ///     Race Set number that would be either 1 or 2
        /// </summary>
        public uint RaceSet { get; set; }

        /// <summary>
        ///     To tell if the patterns include progressive reset values
        /// </summary>
        public bool IncludesProgressiveResetValues { get; set; }

        /// <summary>
        ///   extra winnings  for the winning pattern
        /// </summary>
        public double ExtraWinnings { get; set; }

        /// <summary>
        ///   Guaranteed Credits for the winning pattern
        /// </summary>
        public long GuaranteedCredits { get; set; }
    }
}