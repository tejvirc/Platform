namespace Aristocrat.Monaco.Bingo.Common.GameOverlay
{
    using System;
    using System.Text;
    using Newtonsoft.Json;

    /// <summary>
    ///     Represent a bingo card.
    /// </summary>
    [Serializable]
    public class BingoCard
    {
        [JsonConstructor]
        public BingoCard(BingoNumber[,] numbers, uint serialNumber, int? initialDaubedBits, int daubedBits, bool isGameEndWin, bool isGolden = false)
        {
            Numbers = numbers;
            SerialNumber = serialNumber;
            InitialDaubedBits = initialDaubedBits;
            DaubedBits = daubedBits;
            IsGameEndWin = isGameEndWin;
            IsGolden = isGolden;
        }

        public BingoCard(uint serialNumber)
        {
            SerialNumber = serialNumber;
        }

        /// <summary>
        ///     a 5x5 array of the bingo card numbers
        /// </summary>
        public BingoNumber[,] Numbers { get; } = new BingoNumber[BingoConstants.BingoCardDimension, BingoConstants.BingoCardDimension];

        /// <summary>
        ///     The serial number of the card
        /// </summary>
        public uint SerialNumber { get; }

        /// <summary>
        ///     Which squares on the card are daubed
        /// </summary>
        public int DaubedBits { get; set; }

        /// <summary>
        ///     The initial squares on the card that
        ///     are daubed at the beginning of the game
        /// </summary>
        public int? InitialDaubedBits { get; set; }

        /// <summary>
        ///     Whether the card has game end win
        /// </summary>
        public bool IsGameEndWin { get; set; }

        /// <summary>
        ///     Whether the card is a golden card
        /// </summary>
        public bool IsGolden { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder($"#{SerialNumber}:{Environment.NewLine} -- -- -- -- --{Environment.NewLine}");
            for (var row = 0; row < BingoConstants.BingoCardDimension; row++)
            {
                for (var col = 0; col < BingoConstants.BingoCardDimension; col++)
                {
                    sb.Append($"|{Numbers[row, col].Number:D2}");
                }

                sb.AppendLine($"|{Environment.NewLine} -- -- -- -- --");
            }

            return sb.ToString();
        }
    }
}
