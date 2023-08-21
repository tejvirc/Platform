namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.UI.OperatorMenu;
    using Contracts;
    using Contracts.Models;
    using Localization.Properties;
    using Models;

    public class GameRtpSummaryViewModel : OperatorMenuSaveViewModelBase
    {
        public GameRtpSummaryViewModel(IReadOnlyCollection<IGameDetail> games, double denomMultiplier)
        {
            if (games is null)
            {
                games = new ReadOnlyCollection<IGameDetail>(new List<IGameDetail>());
            }

            GameTypeItems = new List<GameSummary>();
            GameItemsByType = new Dictionary<GameType, IEnumerable<GameSummary>>();


            var activeDenomGames = games
                .Where(game => game.ActiveDenominations.Any())
                .ToArray();
            var gameTypes = activeDenomGames.Select(g => g.GameType).Distinct().ToList();

            if (gameTypes.Count > 1)
            {
                GameTypeItems.Add(
                    new GameSummary(
                        Localizer.For(CultureFor.Operator).GetString(ResourceKeys.GlobalEGM),
                        GetAverageRtp(games)));
            }

            foreach (var type in gameTypes)
            {
                var typeGames = activeDenomGames.Where(g => g.GameType == type).ToList();

                var name = Localizer.For(CultureFor.Operator).GetString(type.ToString());

                GameTypeItems.Add(new GameSummary(name, GetAverageRtp(typeGames)));

                GameItemsByType.Add(
                    type,
                    CreateGameSummaries(typeGames, denomMultiplier));
            }

            DenomItems = CreateGameSummaries(activeDenomGames, denomMultiplier);
        }

        public List<GameSummary> GameTypeItems { get; }

        public List<GameSummary> DenomItems { get; }

        public Dictionary<GameType, IEnumerable<GameSummary>> GameItemsByType { get; }

        public bool HasDenomItems => DenomItems.Any();

        public bool HasGameTypeItems => GameTypeItems.Any();

        public bool HasMoreThanOneGameType => GameTypeItems.Count > 1;

        private static decimal GetAverageRtp(IReadOnlyCollection<IGameProfile> games)
        {
            return !games.Any()
                ? 0
                : games.Average(g => (g.MaximumPaybackPercent + g.MinimumPaybackPercent) / 2);
        }

        private static List<GameSummary> CreateGameSummaries(
            IReadOnlyCollection<IGameDetail> games,
            double denomMultiplier)
        {
            return games
                .SelectMany(g => g.ActiveDenominations)
                .Distinct()
                .OrderBy(denom => denom)
                .Select(denom => CreateGameSummary(games, denom, denomMultiplier))
                .ToList();
        }

        private static GameSummary CreateGameSummary(
            IReadOnlyCollection<IGameDetail> games,
            long denomValue,
            double denomMultiplier)
        {
            var formattedDenomValue = (denomValue / denomMultiplier).FormattedCurrencyString();
            var matchingActiveDenomGames = games.Where(g => g.ActiveDenominations.Contains(denomValue)).ToList();
            var averageRtp = GetAverageRtp(matchingActiveDenomGames);

            return new GameSummary(formattedDenomValue, averageRtp);
        }
    }
}