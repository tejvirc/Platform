namespace Aristocrat.Bingo.Client.Messages.GamePlay
{
    public class CardPlayed
    {
        public CardPlayed(uint serialNumber, int bitPattern, bool isGameEndWin, bool isGolden = false)
        {
            SerialNumber = serialNumber;
            BitPattern = bitPattern;
            IsGameEndWin = isGameEndWin;
            IsGolden = isGolden;
        }

        /// <summary>
        ///     The serial number for the card
        /// </summary>
        public uint SerialNumber { get; }

        /// <summary>
        ///     Top row, left to right, is bits 24, 23, 22, 21, 20.
        ///     Subsequent rows are subsequent blocks of 5 bits.
        /// </summary>
        public int BitPattern { get; }

        /// <summary>
        ///     Whether this card has obtained the game end win pattern
        /// </summary>
        public bool IsGameEndWin { get; }

        /// <summary>
        ///     Whether this card is a golden card
        /// </summary>
        public bool IsGolden { get; }
    }
}