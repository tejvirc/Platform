namespace Aristocrat.Monaco.Bingo.UI.Models
{
    using System.Collections;
    using System.Collections.Generic;
    using Common.GameOverlay;
    using OverlayServer.Data.Bingo;
    using BingoPattern = Common.GameOverlay.BingoPattern;

    public class BingoInstanceModel
    {
        public BingoCard Card { get; set; }

        public int InstanceNumber { get; set; } = 1;

        public bool Enabled { get; set; } = true;

        public bool Visible { get; set; } = true;

        public IReadOnlyList<BingoCardNumber> BingoCardNumbers { get; set; } = new List<BingoCardNumber>();

        public IReadOnlyList<BingoPattern> BingoPatterns { get; set; } = new List<BingoPattern>();

        public IReadOnlyList<BingoPattern> CyclingPatterns { get; set; } = new List<BingoPattern>();

        public void DaubBingoCard(int daubs)
        {
            // daub bingo card numbers based on daub pattern encoded in an integer.
            var daubed = new BitArray(new[] { daubs });
            for (var i = 0; i < BingoCardNumbers.Count; i++)
            {
                BingoCardNumbers[i].State =
                    daubed[i] ? BingoCardNumber.DaubState.NonPatternDaub : BingoCardNumber.DaubState.NoDaub;
            }
        }
    }
}
