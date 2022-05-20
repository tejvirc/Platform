namespace Aristocrat.Monaco.Bingo.Common.GameOverlay
{
    using System;
    using System.Text;
    using Newtonsoft.Json;

    public class BingoPattern
    {
        [JsonConstructor]
        public BingoPattern(
            string name,
            int patternId,
            uint cardSerial,
            long winAmount,
            int ballQuantity,
            int paytableId,
            bool isGameEndWin,
            int bitFlags,
            int winIndex)
        {
            Name = name;
            PatternId = patternId;
            CardSerial = cardSerial;
            WinAmount = winAmount;
            BallQuantity = ballQuantity;
            PaytableId = paytableId;
            IsGameEndWin = isGameEndWin;
            BitFlags = bitFlags;
            Flags = bitFlags.BitPatternToFlags();
            WinIndex = winIndex;
        }

        /// <summary>
        ///     Name
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets the pattern Id
        /// </summary>
        public int PatternId { get; }

        /// <summary>
        ///     Card serial number
        /// </summary>
        public uint CardSerial { get; }

        /// <summary>
        ///     Whether or not the pattern is for the GEW
        /// </summary>
        public bool IsGameEndWin { get; }

        /// <summary>
        ///     Gets  the win amount for the pattern
        /// </summary>
        public long WinAmount { get; set; }

        /// <summary>
        ///     Gets the ball quantity for this pattern
        /// </summary>
        public int BallQuantity { get; }

        /// <summary>
        ///     Gets the paytable Id for this win
        /// </summary>
        public int PaytableId { get; }

        /// <summary>
        ///     Card pattern as a 2-D array.
        /// </summary>
        [JsonIgnore]
        public bool[,] Flags { get; }

        /// <summary>
        ///     Gets the bit masked flags for the bingo pattern
        /// </summary>
        public int BitFlags { get; }

        /// <summary>
        ///     The win index of the pattern
        /// </summary>
        public int WinIndex { get; }

        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) &&
                   (ReferenceEquals(this, obj) || obj.GetType() == GetType() && Equals((BingoPattern)obj));
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Name != null ? Name.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ PatternId;
                hashCode = (hashCode * 397) ^ CardSerial.GetHashCode();
                hashCode = (hashCode * 397) ^ IsGameEndWin.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder($"{Name}:{Environment.NewLine} -- -- -- -- --{Environment.NewLine}");
            for (var row = 0; row < BingoConstants.BingoCardDimension; row++)
            {
                for (var col = 0; col < BingoConstants.BingoCardDimension; col++)
                {
                    sb.Append($"|{(Flags[row, col] ? "XX" : "  ")}");
                }

                sb.AppendLine($"|{Environment.NewLine} -- -- -- -- --");
            }

            return sb.ToString();
        }

        private bool Equals(BingoPattern other)
        {
            return Name == other.Name && PatternId == other.PatternId && CardSerial == other.CardSerial &&
                   IsGameEndWin == other.IsGameEndWin;
        }
    }
}