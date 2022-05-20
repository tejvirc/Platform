namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using System.Linq;
    using Contracts;
    using Contracts.Progressives;
    using Progressives;

    /// <summary>
    ///     Claim jackpot command handler
    /// </summary>
    public class ClaimJackpotCommandHandler : ICommandHandler<ClaimJackpot>
    {
        private readonly IProgressiveGameProvider _progressiveGame;
        private readonly IGameDiagnostics _gameDiagnostics;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ClaimJackpotCommandHandler" /> class.
        /// </summary>
        public ClaimJackpotCommandHandler(IProgressiveGameProvider progressiveGame, IGameDiagnostics gameDiagnostics)
        {
            _progressiveGame = progressiveGame ?? throw new ArgumentNullException(nameof(progressiveGame));
            _gameDiagnostics = gameDiagnostics ?? throw new ArgumentNullException(nameof(gameDiagnostics));
        }

        /// <inheritdoc />
        public void Handle(ClaimJackpot command)
        {
            if (_gameDiagnostics.IsActive && _gameDiagnostics.Context is IDiagnosticContext<IGameHistoryLog> context)
            {
                // For replay we're going to pull solely from the game history
                command.Results =
                    context.Arguments
                        .Jackpots.Where(j => command.TransactionIds.Contains(j.TransactionId))
                        .Select(j => new ClaimResult { LevelId = j.LevelId, WinAmount = j.WinAmount });

                return;
            }

            command.Results = _progressiveGame.ClaimProgressiveLevel(command.PoolName, command.TransactionIds);
        }
    }
}