namespace Aristocrat.Bingo.Client.Messages.GamePlay
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage(
        "ReSharper",
        "UnusedAutoPropertyAccessor.Global",
        Justification = "This gets set when created from server message and could be used by message handler in the future")]
    public class GameOutcomeWinDetails
    {
        public GameOutcomeWinDetails(long totalWin, string progressiveLevels, IReadOnlyCollection<WinResult> winResults)
        {
            TotalWin = totalWin;
            ProgressiveLevels = progressiveLevels;
            WinResults = winResults;
        }

        public long TotalWin { get; }

        public string ProgressiveLevels { get; }

        public IReadOnlyCollection<WinResult> WinResults { get; }
    }
}