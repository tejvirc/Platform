namespace Aristocrat.Monaco.Bingo.Common.GameOverlay
{
    using System;
    using System.Text;
    using Newtonsoft.Json;

    /// <summary>
    ///     Represent a bingo card.
    /// </summary>
    public class BingoCard
    {
        [JsonConstructor]
        public BingoCard(BingoNumber[,] numbers, uint serialNumber, int daubedBits, bool isGameEndWin)
        {
            Numbers = numbers;
            SerialNumber = serialNumber;
            DaubedBits = daubedBits;
            IsGameEndWin = isGameEndWin;
        }

        public BingoCard()
        {
        }

        public BingoNumber[,] Numbers { get; } = new BingoNumber[BingoConstants.BingoCardDimension, BingoConstants.BingoCardDimension];

        public uint SerialNumber { get; set; }

        public int DaubedBits { get; set; }

        public bool IsGameEndWin { get; set; }

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
