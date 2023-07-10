namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.Progressives;
    using Progressives;

    public class GetAllEnabledJackpotValuesCommandHandler : ICommandHandler<GetAllEnabledJackpotValues>
    {

        private readonly IProgressiveLevelProvider _progressiveLevelProvider;
        private readonly IGameProvider _gameProvider;
        public GetAllEnabledJackpotValuesCommandHandler(IProgressiveLevelProvider progressiveLevelProvider, IGameProvider gameProvider)
        {
            _progressiveLevelProvider = progressiveLevelProvider;
            _gameProvider = gameProvider;
        }

        public void Handle(GetAllEnabledJackpotValues command)
        {
            // Single Call, Single Game and PoolName and Denom
            // Game and PoolName and Denom -- Mandatory

            // ThemeId - String Name

            var gameName = command.GameName;
            var poolName = command.PoolName;
            var denom = command.Denomination;

            var enabledGames = _gameProvider.GetEnabledGames()
                .Where(game => game.ThemeId == gameName)
                .Where(game => game.BetLinePresetList.Any(betLineOptions => betLineOptions.BetOption.Equals(game.ActiveBetOption) && betLineOptions.LineOption.Equals(game.ActiveLineOption)))
                .Select(game => (game.Id, game.ActiveBetOption));

            var gameBetLine = enabledGames.Select(games => games.ActiveBetOption.Name);
            var leelLines = _progressiveLevelProvider.GetProgressiveLevels().Select(x => x.BetOption);

            command.JackpotValues = _progressiveLevelProvider.GetProgressiveLevels()
                .Where(level => enabledGames.Select(games => games.Id).Contains(level.GameId))
                .Where(level => level.Denomination.Contains((long)denom))
                .Where(level => level.HasAssociatedBetLinePreset ? enabledGames.Select(games => games.ActiveBetOption.Name).Contains(level.BetOption) : true)
                .Where(level => level.ProgressivePackName.Equals(poolName))

                .ToDictionary(levels => levels.LevelId, levels => levels.CurrentValue);
        }
    }
}
