namespace Aristocrat.Bingo.Client.Messages.GamePlay
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage(
        "ReSharper",
        "UnusedAutoPropertyAccessor.Global",
        Justification = "This gets set when created from server message and could be used by message handler in the future")]
    public class GameOutcome : IResponse
    {
        public GameOutcome(
            ResponseCode code,
            GameOutcomeWinDetails winDetails,
            GameOutcomeGameDetails gameDetails,
            GameOutcomeBingoDetails bingoDetails,
            bool isSuccessful,
            bool isFinal)
        {
            ResponseCode = code;
            WinDetails = winDetails ?? throw new ArgumentNullException(nameof(winDetails));
            GameDetails = gameDetails ?? throw new ArgumentNullException(nameof(gameDetails));
            BingoDetails = bingoDetails ?? throw new ArgumentNullException(nameof(bingoDetails));
            IsSuccessful = isSuccessful;
            IsFinal = isFinal;
        }

        public ResponseCode ResponseCode { get; }

        public GameOutcomeWinDetails WinDetails { get; }

        public GameOutcomeGameDetails GameDetails { get; }

        public GameOutcomeBingoDetails BingoDetails { get; }

        public bool IsFinal { get; }

        public bool IsSuccessful { get; }
    }
}