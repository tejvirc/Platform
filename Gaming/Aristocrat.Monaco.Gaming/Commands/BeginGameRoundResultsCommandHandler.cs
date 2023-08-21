namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using Contracts;
    using Contracts.Models;
    using Contracts.Progressives;

    public class BeginGameRoundResultsCommandHandler : ICommandHandler<BeginGameRoundResults>
    {
        private readonly IGameDiagnostics _gameDiagnostics;
        private readonly IGameHistory _gameHistory;

        public BeginGameRoundResultsCommandHandler(IGameDiagnostics gameDiagnostics, IGameHistory gameHistory)
        {
            _gameDiagnostics = gameDiagnostics ?? throw new ArgumentNullException(nameof(gameDiagnostics));
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
        }

        public void Handle(BeginGameRoundResults command)
        {
            if (_gameDiagnostics.IsActive)
            {
                return;
            }

            _gameHistory.LogGameRoundDetails(new GameRoundDetails { PresentationIndex = command.PresentationIndex });
        }
    }
}