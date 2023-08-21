namespace Aristocrat.Monaco.Hhr.UI.Models
{
    public class WinningOddsModel
    {
        /// <summary>
        /// Horse Number for a particular winning odds
        /// </summary>
        public int HorseNo { get; set; }

        /// <summary>
        /// Winning odds for the given horse number
        /// </summary>
        public int WinningOdds { get; set; }
        public WinningOddsModel(int horseNo, int winningOdds)
        {
            HorseNo = horseNo;
            WinningOdds = winningOdds;
        }
    }
}
