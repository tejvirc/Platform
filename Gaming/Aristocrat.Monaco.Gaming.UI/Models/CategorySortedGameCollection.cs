namespace Aristocrat.Monaco.Gaming.UI.Models
{
    using System.Collections.Generic;
    using System.Linq;
    using Contracts.Configuration;
    using Contracts;
    using Contracts.Models;

    public class CategorySortedGameCollection
    {
        private readonly IGameProvider _gameProvider;
        private readonly IGameConfigurationProvider _configProvider;
        private const int MinimumLightningLinkGameCount = 2;

        public CategorySortedGameCollection(IGameProvider gameProvider,
            IGameConfigurationProvider configProvider)
        {
            _gameProvider = gameProvider;
            _configProvider = configProvider;
        }

        public IEnumerable<IGameDetail> GetEnabledGamesSortedByCategory()
        {
            var lightningLinkGames = new Dictionary<string, List<IGameDetail>>();

            var result = new Dictionary<GameCategory, List<IGameDetail>>();

            foreach (var game in _gameProvider.GetEnabledGames())
            {
                if (game.Category == GameCategory.LightningLink)
                {
                    if (!lightningLinkGames.ContainsKey(game.ThemeId))
                    {
                        lightningLinkGames.Add(game.ThemeId, new List<IGameDetail>());
                    }

                    lightningLinkGames[game.ThemeId].Add(game);

                    continue;
                }

                if (!result.ContainsKey(game.Category))
                {
                    result.Add(game.Category, new List<IGameDetail>());
                }

                result[game.Category].Add(game);
            }

            if (!IncludeLightningLinkGames(lightningLinkGames))
            {
                return result.Values.SelectMany(v => v);
            }

            result.Add(
                GameCategory.LightningLink,
                lightningLinkGames.Values.SelectMany(v => v).ToList()
            );

            return result.Values.SelectMany(v => v);
        }

        private bool IncludeLightningLinkGames
            (Dictionary<string, List<IGameDetail>> lightningLinkGames)
        {
            if (lightningLinkGames.Count < MinimumLightningLinkGameCount)
            {
                return false;
            }

            foreach (var kvp in lightningLinkGames)
            {
                var activeVariations = kvp.Value
                    .Where(v => v.Enabled && v.EgmEnabled)
                    .Select(v => v.VariationId)
                    .OrderBy(v => v)
                    .ToArray();

                if (!activeVariations.Any())
                {
                    return false;
                }

                var gamePackVariations =
                    _configProvider.GetActive(kvp.Key)?
                    .RestrictionDetails?
                    .Mapping?
                    .Where(m => m.Active)
                    .Select(m => m.VariationId)
                    .OrderBy(v => v) ?? Enumerable.Empty<string>();

                if (!gamePackVariations.SequenceEqual(activeVariations))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
