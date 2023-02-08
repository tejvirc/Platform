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
    using Contracts.Rtp;
    using Kernel;
    using Localization.Properties;
    using Models;

    public class GameRtpSummaryViewModel : OperatorMenuSaveViewModelBase
    {
        private readonly IRtpService _rtpService;

        public GameRtpSummaryViewModel(IReadOnlyCollection<IGameDetail> games, double denomMultiplier)
        {
            _rtpService = ServiceManager.GetInstance().GetService<IRtpService>();
            games ??= new ReadOnlyCollection<IGameDetail>(new List<IGameDetail>());

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

        private decimal GetAverageRtp(IReadOnlyCollection<IGameProfile> games)
        {
            if (!games.Any())
            {
                return 0m; 
            }

            var rtpReport = _rtpService.GetRtpReport(games.ToArray());

            rtpReport.GetRtpBreakdowns()


            //var gameThemes = games.Select(game => game.ThemeId).Distinct();
            //var rtpReportsByTheme = new Dictionary<string, RtpReport>();

            //foreach (var themeId in gameThemes)
            //{
            //    rtpReportsByTheme[themeId] = _rtpService.GetRtpReportForGameTheme(themeId);
            //}

            //var averageRtp = games.Average(game =>
            //{
            //    var rtpBreakdown = rtpReportsByTheme[game.ThemeId].GetTotalVariationRtp(game.VariationId);
            //    var totalRtp = rtpBreakdown.Rtp;
            //    return (totalRtp.Minimum + totalRtp.Maximum) / 2.0m;
            //});

            //return averageRtp;
        }

        private List<GameSummary> CreateGameSummaries(
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

        private GameSummary CreateGameSummary(
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