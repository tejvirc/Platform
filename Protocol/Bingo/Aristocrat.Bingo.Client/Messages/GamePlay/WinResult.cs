namespace Aristocrat.Bingo.Client.Messages.GamePlay
{
    using System.Collections.Generic;

    /// <summary>
    ///     Data for one win result
    /// </summary>
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
            int winIndex,
            IEnumerable<string> progressiveWins)
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
            ProgressiveWins = progressiveWins;
        }

        /// <summary>Gets pattern id</summary>
        public int PatternId { get; }

        /// <summary>Gets the payout amount of the win</summary>
        public long Payout { get; }

        /// <summary>Gets the ball quantity for the win</summary>
        public int BallQuantity { get; }

        /// <summary>
        ///     The bit pattern on the bingo card for the win.
        ///     Top row, left to right, is bits 24, 23, 22, 21, 20.
        ///     Subsequent rows are subsequent blocks of 5 bits.
        /// </summary>
        public int BitPattern { get; }

        /// <summary>Gets the paytable id</summary>
        public int PaytableId { get; }

        /// <summary>Gets the pattern name</summary>
        public string PatternName { get; }

        /// <summary>Gets the card serial</summary>
        public uint CardSerial { get; }

        /// <summary>Gets if this is a game end win</summary>
        public bool IsGameEndWin { get; }

        /// <summary>Gets the win index</summary>
        public int WinIndex { get; }

        /// <summary>Gets the progressive wins.</summary>
        public IEnumerable<string> ProgressiveWins { get; }
    }
}