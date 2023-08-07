namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using System.Linq;
    using Application.Contracts.Extensions;
    using Contracts;
    using Contracts.Progressives;
    using Progressives;

    /// <summary>
    ///     Command handler for the <see cref="GetJackpotValues" /> command.
    /// </summary>
    public class GetJackpotValuesCommandHandler : ICommandHandler<GetJackpotValues>
    {
        private readonly IGameDiagnostics _gameDiagnostics;
        private readonly IProgressiveGameProvider _progressiveGame;
        private readonly IGameProvider _gameProvider;
        private readonly IProgressiveLevelProvider _progressiveLevelProvider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetJackpotValuesCommandHandler" /> class.
        /// </summary>
        public GetJackpotValuesCommandHandler(IGameDiagnostics diagnostics, IProgressiveGameProvider progressiveGame, IGameProvider gameProvider, IProgressiveLevelProvider progressiveLevelProvider)
        {
            _gameDiagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
            _progressiveGame = progressiveGame ?? throw new ArgumentNullException(nameof(progressiveGame));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _progressiveLevelProvider = progressiveLevelProvider ?? throw new ArgumentNullException(nameof(progressiveLevelProvider));
        }

        /// <inheritdoc />
        public void Handle(GetJackpotValues command)
        {
            if (!string.IsNullOrEmpty(command.GameName))
            {
                GetJackpotValuesForParticularDenomAndGame(command);
                return;
            }

            if (_gameDiagnostics.IsActive && _gameDiagnostics.Context is IDiagnosticContext<IGameHistoryLog> context)
            {
                if (context.Arguments.JackpotSnapshot != null)
                {
                    command.JackpotValues =
                        context.Arguments.JackpotSnapshot.ToDictionary(x => x.LevelId, x => x.Value);
                }
            }
            else
            {
                command.JackpotValues = _progressiveGame.GetProgressiveLevel(command.PoolName, command.Recovering);
            }
        }

        private void GetJackpotValuesForParticularDenomAndGame(GetJackpotValues command)
        {
            var gameName = command.GameName;
            var poolName = command.PoolName;
            var denom = ((long)command.Denomination).CentsToMillicents();

            var enabledGames = _gameProvider.GetEnabledGames()
                .Where(game => game.ThemeName == gameName) // Get Game based on Product Name
                .Where(game => game.BetLinePresetList.Any(betLineOptions => betLineOptions.BetOption.Equals(game.ActiveBetOption) && betLineOptions.LineOption.Equals(game.ActiveLineOption)))
                .Select(game => (game.Id, game.ActiveBetOption));

            command.JackpotValues = _progressiveLevelProvider.GetProgressiveLevels()
                .Where(level => enabledGames.Select(games => games.Id).Contains(level.GameId))
                .Where(level => level.Denomination.Contains(denom))
                .Where(level => !level.HasAssociatedBetLinePreset || enabledGames.Select(games => games.ActiveBetOption.Name).Contains(level.BetOption))
                .Where(level => level.ProgressivePackName.Equals(poolName, StringComparison.Ordinal))
                .ToDictionary(levels => levels.LevelId, levels => levels.CurrentValue);
        }
    }
}