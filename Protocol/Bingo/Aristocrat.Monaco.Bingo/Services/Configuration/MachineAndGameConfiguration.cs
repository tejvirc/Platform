namespace Aristocrat.Monaco.Bingo.Services.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Common;
    using Common.Storage.Model;
    using Gaming.Contracts;
    using Gaming.Contracts.Configuration;
    using Kernel;
    using Newtonsoft.Json;
    using Quartz.Util;

    public class MachineAndGameConfiguration : BaseConfiguration
    {
        private const int InvalidResult = -1;

        private readonly IGameProvider _gameProvider;
        private readonly IConfigurationProvider _restrictionProvider;

        public MachineAndGameConfiguration(
            IPropertiesManager propertiesManager,
            ISystemDisableManager systemDisableManager,
            IGameProvider gameProvider,
            IConfigurationProvider restrictionProvider)
            : base(propertiesManager, systemDisableManager)
        {
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _restrictionProvider = restrictionProvider ?? throw new ArgumentNullException(nameof(restrictionProvider));
            ConfigurationConversion =
                new Dictionary<string, (string, Func<string, object>)>
                {
                    { MachineAndGameConfigurationConstants.MachineSerial, (ApplicationConstants.MachineId, val => Convert.ToUInt32(val))},
                    { MachineAndGameConfigurationConstants.LocationId, (ApplicationConstants.Location, null)},
                    { MachineAndGameConfigurationConstants.LocationZoneId, (ApplicationConstants.Zone, null)},
                    { MachineAndGameConfigurationConstants.LocationBank, (ApplicationConstants.Bank, null)},
                    { MachineAndGameConfigurationConstants.LocationPosition, (ApplicationConstants.Position, null)},
                    { MachineAndGameConfigurationConstants.BingoCardPlacement, (BingoConstants.BingoCardPlacement, null)},
                    { MachineAndGameConfigurationConstants.DispBingoCard, (BingoConstants.DisplayBingoCardEgm, null)},
                    { MachineAndGameConfigurationConstants.BingoHelpUri, (BingoConstants.BingoHelpUri, null)}
                };

            RequiredSettings =
                new HashSet<string>
                {
                    MachineAndGameConfigurationConstants.GamesConfigured
                };
        }

        protected override void AdditionalConfiguration(BingoServerSettingsModel model, string name, string value)
        {
            switch (name)
            {
                case MachineAndGameConfigurationConstants.LocationZoneId:
                    model.ZoneId = value;
                    break;
                case MachineAndGameConfigurationConstants.LocationBank:
                    model.BankId = value;
                    break;
                case MachineAndGameConfigurationConstants.LocationPosition:
                    model.Position = value;
                    break;
                case MachineAndGameConfigurationConstants.BingoCardPlacement:
                    model.BingoCardPlacement = value;
                    break;
                case MachineAndGameConfigurationConstants.DispBingoCard when !UsingGlobalSetting(value):
                    model.DisplayBingoCard = StringToBool(value);
                    break;
                case MachineAndGameConfigurationConstants.HideBingoCardWhenInactive when !UsingGlobalSetting(value):
                    model.HideBingoCardWhenInactive = StringToBool(value);
                    break;
                case MachineAndGameConfigurationConstants.BingoHelpUri:
                    break;
                case MachineAndGameConfigurationConstants.GamesConfigured when string.IsNullOrEmpty(model.ServerGameConfiguration):
                    model.ServerGameConfiguration = value;
                    var configured = JsonConvert.DeserializeObject<List<ServerGameConfiguration>>(value);
                    var results = ConfigureGames(configured).ToList();
                    model.GamesConfigurationText = JsonConvert.SerializeObject(results);
                    break;
                case MachineAndGameConfigurationConstants.GamesConfigured:
                    // Do nothing we already setup the games
                    break;
                default:
                    LogUnhandledSetting(name, value);
                    break;
            }
        }

        private IEnumerable<BingoGameConfiguration> ConfigureGames(IEnumerable<ServerGameConfiguration> configured)
        {
            var gameDetails = _gameProvider.GetGames();
            var gameConfigurations = configured.Select(
                    c => (
                        GameDetails: gameDetails.First(
                            d => d.CdsThemeId == c.GameTitleId && d.SupportedDenominations.Contains(c.Denomination.CentsToMillicents())),
                        Settings: c))
                .GroupBy(x => (x.GameDetails.Id, x.GameDetails.ThemeId));

            foreach (var gameConfiguration in gameConfigurations)
            {
                _gameProvider.SetActiveDenominations(
                    gameConfiguration.Key.Id,
                    gameConfiguration.Select(x => x.Settings.Denomination.CentsToMillicents()).ToList());

                foreach (var (gameDetail, setting) in gameConfiguration)
                {
                    yield return setting.ToGameConfiguration(gameDetail);
                }
            }
        }

        protected override bool IsSettingInvalid(string name, string value)
        {
            return name switch
            {
                MachineAndGameConfigurationConstants.GamesConfigured => !ValidGamesConfiguration(value),
                // must be > 0
                MachineAndGameConfigurationConstants.MachineSerial => (long.TryParse(value, out var result) ? result : InvalidResult) <= 0,
                // must be >= 0
                MachineAndGameConfigurationConstants.LocationId => (int.TryParse(value, out var result1) ? result1 : InvalidResult) < 0,
                // must be >= 0
                MachineAndGameConfigurationConstants.MachineTypeId => (int.TryParse(value, out var result2) ? result2 : InvalidResult) < 0,
                // must be >= 0
                MachineAndGameConfigurationConstants.CreditsManager => (int.TryParse(value, out var result3) ? result3 : InvalidResult) < 0,
                // valid values are "EGM Setting" or "Top Screen"
                MachineAndGameConfigurationConstants.BingoCardPlacement => ValidateBingoCardPlacement(value),
                // valid values are "Use Global Settings" or any bool
                MachineAndGameConfigurationConstants.DispBingoCard =>
                    !(UsingGlobalSetting(value) || IsBooleanValue(value)),
                MachineAndGameConfigurationConstants.HideBingoCardWhenInactive =>
                    !(UsingGlobalSetting(value) || IsBooleanValue(value)),
                MachineAndGameConfigurationConstants.BingoHelpUri =>
                    value.IsNullOrWhiteSpace() || !Uri.IsWellFormedUriString(value, UriKind.RelativeOrAbsolute),
                _ => false,
            };
        }

        protected override bool SettingChangedThatRequiresNvRamClear(string name, string value, BingoServerSettingsModel model)
        {
            return name switch
            {
                MachineAndGameConfigurationConstants.GamesConfigured =>
                    !string.IsNullOrEmpty(model.ServerGameConfiguration) &&
                    !string.Equals(value, model.ServerGameConfiguration, StringComparison.InvariantCultureIgnoreCase),
                _ => false
            };
        }

        private static bool UsingGlobalSetting(string value)
        {
            return string.Equals(
                value,
                MachineAndGameConfigurationConstants.UseGlobalSettings,
                StringComparison.OrdinalIgnoreCase);
        }

        private static bool ValidateBingoCardPlacement(string value)
        {
            return !string.Equals(value, MachineAndGameConfigurationConstants.EgmSetting, StringComparison.Ordinal) &&
                   !string.Equals(value, MachineAndGameConfigurationConstants.TopScreen, StringComparison.Ordinal);
        }

        private bool ValidGamesConfiguration(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            try
            {
                var result = JsonConvert.DeserializeObject<List<ServerGameConfiguration>>(value);
                var gameDetails = _gameProvider.GetGames();

                var gameConfigurations = result.Select(
                        c => (
                            GameDetails: gameDetails.FirstOrDefault(
                                d => d.CdsThemeId == c.GameTitleId && d.SupportedDenominations.Contains(c.Denomination.CentsToMillicents())),
                            Settings: c))
                    .GroupBy(x => x.GameDetails?.Id ?? -1);
                return result.Any() &&
                       gameConfigurations.All(x => x.Key != -1 && IsConfigurationValid(x.ToList()));
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool IsConfigurationValid(
            IReadOnlyCollection<(IGameDetail GameDetails, ServerGameConfiguration Settings)> configurations)
        {
            var gameDetail = configurations.First().GameDetails;
            var denoms = configurations.Select(c => c.Settings).ToList();
            var restrictions = _restrictionProvider.GetByThemeId(gameDetail.ThemeId).Select(x => x.RestrictionDetails).Where(
                x => x.MaxDenomsEnabled is not null || x.Mapping.Any(m => m.Active)).ToList();
            return restrictions.Count == 0 || restrictions.Any(
                x => x.MaxDenomsEnabled is not null && x.MaxDenomsEnabled >= denoms.Count ||
                     x.Mapping.Count(m => m.Active) == denoms.Count && x.Mapping.All(
                         m => m.Active && denoms.Any(d => d.Denomination.CentsToMillicents() == m.Denomination)));
        }
    }
}
