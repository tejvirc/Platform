namespace Aristocrat.Monaco.Bingo.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Application.Contracts.Extensions;
    using Common;
    using Common.Exceptions;
    using Common.Extensions;
    using Common.Storage.Model;
    using Configuration;
    using Gaming.Contracts;
    using Gaming.Contracts.Configuration;
    using log4net;

    public class BingoPaytableInstaller : IBingoPaytableInstaller
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IServerPaytableInstaller _serverPaytableInstaller;
        private readonly IGameProvider _gameProvider;
        private readonly IConfigurationProvider _restrictionProvider;

        public BingoPaytableInstaller(
            IServerPaytableInstaller serverPaytableInstaller,
            IGameProvider gameProvider,
            IConfigurationProvider restrictionProvider)
        {
            _serverPaytableInstaller = serverPaytableInstaller ?? throw new ArgumentNullException(nameof(serverPaytableInstaller));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _restrictionProvider = restrictionProvider ?? throw new ArgumentNullException(nameof(restrictionProvider));
        }

        public IEnumerable<BingoGameConfiguration> ConfigureGames(IEnumerable<ServerGameConfiguration> gameConfigurations)
        {
            var configurations = gameConfigurations.ToList();
            foreach (var gameConfiguration in GetGameConfigurations(configurations))
            {
                var gameDetails = InstallGame(gameConfiguration);
                _gameProvider.SetActiveDenominations(gameDetails.Id, configurations.Select(x => x.Denomination.CentsToMillicents()).ToList());
                var sideBetGames = configurations.Select(x => x.SideBetGames).FirstOrDefault();
                if (sideBetGames is not null)
                {
                    var subGames =
                        sideBetGames.Select(
                            subGame => new SubGameConfiguration()
                            {
                                GameTitleId = subGame.GameTitleId,
                                Denomination = subGame.Denomination
                            }).ToList();

                    _serverPaytableInstaller.InstallSubGames(gameDetails.Id, subGames);
                }
                
                foreach (var (gameDetail, setting) in gameConfiguration)
                {
                    yield return setting.ToGameConfiguration(gameDetail);
                }
            }
        }

        public IEnumerable<BingoGameConfiguration> UpdateConfiguration(IEnumerable<ServerGameConfiguration> gameConfigurations)
        {
            foreach (var (gameDetail, setting) in GetGameConfigurations(gameConfigurations).SelectMany(x => x))
            {
                yield return setting.ToGameConfiguration(gameDetail);
            }
        }

        public bool IsConfigurationValid(IEnumerable<ServerGameConfiguration> result)
        {
            var gameDetails = _serverPaytableInstaller.GetAvailableGames();
            var serverGameConfigurations = result.ToList();
            var gameConfigurations = serverGameConfigurations.Select(
                    c => (
                        GameDetails: gameDetails.FirstOrDefault(
                            d => d.GetBingoTitleId() == c.GameTitleId.ToString() && d.SupportedDenominations.Contains(c.Denomination.CentsToMillicents())),
                        Settings: c))
                .GroupBy(x => x.GameDetails?.Id ?? -1);
            return serverGameConfigurations.Any() &&
                   serverGameConfigurations.All(IsHelpUriValid) &&
                   gameConfigurations.All(x => x.Key != -1 && IsConfigurationValid(x.ToList()));
        }

        private static bool IsHelpUriValid(ServerGameConfiguration configuration)
        {
            var helpUrl = configuration.HelpUrl;
            return string.IsNullOrEmpty(helpUrl) || helpUrl.IsValidHelpUri();
        }

        private IGameDetail InstallGame(IGrouping<(int Id, string ThemeId), (IGameDetail GameDetails, ServerGameConfiguration Settings)> gameConfiguration)
        {
            const decimal rtpScale = 100.0M;
            var (gameDetails, settings) = gameConfiguration.FirstOrDefault();
            if (!(settings?.BetInformationDetails?.Any() ?? false))
            {
                throw new ConfigurationException(
                    "Bet information is missing",
                    ConfigurationFailureReason.InvalidGameConfiguration);
            }

            var wagerCategories = gameDetails.WagerCategories.ToList();
            var gameOptionConfigValues = new ServerPaytableConfiguration
            {
                PaytableId = settings.PaytableId.ToString(),
                MaximumPaybackPercent = settings.BetInformationDetails.Max(x => x.Rtp * rtpScale),
                MinimumPaybackPercent = settings.BetInformationDetails.Min(x => x.Rtp * rtpScale),
                DenominationConfigurations = gameConfiguration.Select(
                    x => new DenominationConfiguration { Value = x.Settings.Denomination.CentsToMillicents() }).ToArray()
            };

            if (wagerCategories.Count > 1)
            {
                gameOptionConfigValues.WagerCategoryOptions = settings.BetInformationDetails.Select(
                    (betDetails, index) => new WagerCategoryConfiguration
                    {
                        Id = wagerCategories[index].Id, TheoPaybackPercentRtp = betDetails.Rtp * rtpScale
                    }).ToArray();
            }
            else if (wagerCategories.Count == 1)
            {
                var maxBetInformation = settings.BetInformationDetails.OrderByDescending(x => x.Bet).First();
                gameOptionConfigValues.WagerCategoryOptions = new []
                {
                    new WagerCategoryConfiguration
                    {
                        Id = wagerCategories.First().Id,
                        TheoPaybackPercentRtp = maxBetInformation.Rtp * rtpScale
                    }
                };
            }

            var gameDetail = _serverPaytableInstaller.InstallGame(gameConfiguration.Key.Id, gameOptionConfigValues);

            if (gameDetail is null)
            {
                throw new ConfigurationException(
                    "Game was unable to be installed",
                    ConfigurationFailureReason.InvalidGameConfiguration);
            }

            return gameDetails;
        }

        private bool IsConfigurationValid(IReadOnlyCollection<(IGameDetail GameDetails, ServerGameConfiguration Settings)> configurations)
        {
            var (gameDetail, setting) = configurations.First();
            var denoms = configurations.Select(c => c.Settings).ToList();
            var restrictions = _restrictionProvider.GetByThemeId(gameDetail.ThemeId).Select(x => x.RestrictionDetails).Where(
                x => x.MaxDenomsEnabled is not null || x.Mapping.Any(m => m.Active)).ToList();
            var validRestrictions = restrictions.Count == 0 || restrictions.Any(
                x => x.MaxDenomsEnabled is not null && x.MaxDenomsEnabled >= denoms.Count ||
                     x.Mapping.Count(m => m.Active) == denoms.Count && x.Mapping.All(
                         m => m.Active && denoms.Any(d => d.Denomination.CentsToMillicents() == m.Denomination)));
            var wagerCategoryCount = gameDetail.WagerCategories.Count();
            var wagerCategoriesMatch =
                wagerCategoryCount <= 1 || wagerCategoryCount == setting.BetInformationDetails.Count;

            Logger.Info($"WagerCategories Match {wagerCategoriesMatch}.  ValidRestrictions={validRestrictions}");
            return validRestrictions && wagerCategoriesMatch;
        }

        private IEnumerable<IGrouping<(int Id, string ThemeId), (IGameDetail GameDetails, ServerGameConfiguration Settings)>> GetGameConfigurations(IEnumerable<ServerGameConfiguration> configured)
        {
            var gameDetails = _serverPaytableInstaller.GetAvailableGames();
            var gameConfigurations = configured.Select(
                    c => (
                        GameDetails: gameDetails.First(
                            d => d.GetBingoTitleId() == c.GameTitleId.ToString() &&
                                 d.SupportedDenominations.Contains(c.Denomination.CentsToMillicents())),
                        Settings: c))
                .GroupBy(x => (x.GameDetails.Id, x.GameDetails.ThemeId));
            return gameConfigurations;
        }
    }
}