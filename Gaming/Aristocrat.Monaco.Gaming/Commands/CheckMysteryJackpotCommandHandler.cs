namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using System.Linq;
    using Contracts;
    using Progressives;


    /// <summary>
    ///     Command handler for the <see cref="CheckMysteryJackpot" /> command.
    /// </summary>
    public class CheckMysteryJackpotCommandHandler : ICommandHandler<CheckMysteryJackpot>
    {
        private readonly IGameDiagnostics _gameDiagnostics;
        private readonly IProgressiveGameProvider _progressiveGame;
        private readonly IMysteryProgressiveProvider _mysteryProgressiveProvider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CheckMysteryJackpotCommandHandler" /> class.
        /// </summary>
        public CheckMysteryJackpotCommandHandler(
            IProgressiveGameProvider progressiveGame,
            IGameDiagnostics gameDiagnostics,
            IMysteryProgressiveProvider mysteryProgressiveProvider
        )
        {
            _progressiveGame = progressiveGame ?? throw new ArgumentNullException(nameof(progressiveGame));
            _gameDiagnostics = gameDiagnostics ?? throw new ArgumentNullException(nameof(gameDiagnostics));

            _mysteryProgressiveProvider = mysteryProgressiveProvider ??
                                          throw new ArgumentNullException(nameof(mysteryProgressiveProvider));
        }

        /// <inheritdoc />
        public void Handle(CheckMysteryJackpot command)
        {
            if (_gameDiagnostics.IsActive && _gameDiagnostics.Context is IDiagnosticContext<IGameHistoryLog> context)
            {
                // For replay the response comes solely from the log
                var jackpots = context.Arguments.Jackpots.Where(t => command.TransactionIds.Contains(t.TransactionId))
                                      .ToList();

                command.Results = command.LevelIds.ToDictionary(
                    id => id,
                    id => jackpots.Exists(jackpot => jackpot.LevelId == id)
                );

                return;
            }

            command.Results = _progressiveGame.GetActiveProgressiveLevels()
                                              .ToDictionary(
                                                  level => level.LevelId,
                                                  level => _mysteryProgressiveProvider.CheckMysteryJackpot(level)
                                              );
        }
    }
}