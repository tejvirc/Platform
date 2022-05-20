namespace Aristocrat.Monaco.Bingo.Services.GamePlay
{
    using Aristocrat.Bingo.Client.Messages.GamePlay;
    using Common.GameOverlay;

    public static class WinResultsExtensions
    {
        public static BingoPattern ToBingoPattern(this WinResult winResult)
        {
            return new BingoPattern(
                winResult.PatternName,
                winResult.PatternId,
                winResult.CardSerial,
                winResult.Payout,
                winResult.BallQuantity,
                winResult.PaytableId,
                winResult.IsGameEndWin,
                winResult.BitPattern,
                winResult.WinIndex);
        }
    }
}