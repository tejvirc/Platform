namespace Aristocrat.Bingo.Client.Messages.GamePlay
{
    public class CardPlayed
    {
        public CardPlayed(uint serialNumber, int bitPattern, bool isGameEndWin)
        {
            SerialNumber = serialNumber;
            BitPattern = bitPattern;
            IsGameEndWin = isGameEndWin;
        }

        public uint SerialNumber { get; }

        /// <summary>
        ///     Top row, left to right, is bits 24, 23, 22, 21, 20.
        ///     Subsequent rows are subsequent blocks of 5 bits.
        /// </summary>
        public int BitPattern { get; }

        public bool IsGameEndWin { get; }
    }
}