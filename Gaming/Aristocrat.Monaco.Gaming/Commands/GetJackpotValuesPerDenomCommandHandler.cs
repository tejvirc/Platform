namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Application.Contracts.Extensions;
    using Contracts;
    using Contracts.Progressives;
    using Progressives;

    public class GetJackpotValuesPerDenomCommandHandler : ICommandHandler<GetJackpotValuesPerDenom>
    {

        private readonly IProgressiveLevelProvider _progressiveLevelProvider;
        private readonly IGameProvider _gameProvider;
        public GetJackpotValuesPerDenomCommandHandler(IProgressiveLevelProvider progressiveLevelProvider, IGameProvider gameProvider)
        {
            _progressiveLevelProvider = progressiveLevelProvider;
            _gameProvider = gameProvider;
        }

        public void Handle(GetJackpotValuesPerDenom command)
        {
            // Single Call, Single Game and PoolName and Denom
            // Game and PoolName and Denom -- Mandatory

            // ThemeId - String Name

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
                .Where(level => level.ProgressivePackName.Equals(poolName))

                .ToDictionary(levels => levels.LevelId, levels => levels.CurrentValue);
        }
    }
}
