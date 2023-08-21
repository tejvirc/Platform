namespace Aristocrat.Monaco.Bingo.Services.GamePlay
{
    using System.Collections.Generic;
    using Aristocrat.Bingo.Client.Messages.GamePlay;

    public class WinResultComparer : IComparer<WinResult>
    {
        public int Compare(WinResult x, WinResult y)
        {
            if (ReferenceEquals(x, y))
            {
                return 0;
            }

            if (y is null)
            {
                return 1;
            }

            if (x is null)
            {
                return -1;
            }

            var isGameEndWinComparison = x.IsGameEndWin.CompareTo(y.IsGameEndWin);
            if (isGameEndWinComparison != 0)
            {
                return isGameEndWinComparison;
            }

            var payoutComparison = x.Payout.CompareTo(y.Payout);
            if (payoutComparison != 0)
            {
                return payoutComparison;
            }

            var patternIdComparison = x.PatternId.CompareTo(y.PatternId);
            if (patternIdComparison != 0)
            {
                return patternIdComparison;
            }

            var ballQuantityComparison = x.BallQuantity.CompareTo(y.BallQuantity);
            if (ballQuantityComparison != 0)
            {
                return ballQuantityComparison;
            }

            return x.WinIndex.CompareTo(y.WinIndex);
        }
    }
}