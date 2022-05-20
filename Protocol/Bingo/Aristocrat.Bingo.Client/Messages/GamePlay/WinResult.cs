namespace Aristocrat.Bingo.Client.Messages.GamePlay
{
    public class WinResult
    {
        public WinResult(
            int patternId,
            long payout,
            int ballQuantity,
            int bitPattern,
            int paytableId,
            string patternName,
            uint cardSerial,
            bool isGameEndWin,
            int winIndex)
        {
            PatternId = patternId;
            Payout = payout;
            BallQuantity = ballQuantity;
            BitPattern = bitPattern;
            PaytableId = paytableId;
            PatternName = patternName;
            CardSerial = cardSerial;
            IsGameEndWin = isGameEndWin;
            WinIndex = winIndex;
        }

        public int PatternId { get; }

        public long Payout { get; }

        public int BallQuantity { get; }

        /// <summary>
        ///     Top row, left to right, is bits 24, 23, 22, 21, 20.
        ///     Subsequent rows are subsequent blocks of 5 bits.
        /// </summary>
        public int BitPattern { get; }

        public int PaytableId { get; }

        public string PatternName { get; }

        public uint CardSerial { get; }

        public bool IsGameEndWin { get; }

        public int WinIndex { get; }
    }
}