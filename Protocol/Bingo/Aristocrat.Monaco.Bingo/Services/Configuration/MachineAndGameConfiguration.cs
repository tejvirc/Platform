namespace Aristocrat.Monaco.Bingo.Services.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts;
    using Common;
    using Common.Storage.Model;
    using Gaming.Contracts;
    using Kernel;
    using Newtonsoft.Json;

    public class MachineAndGameConfiguration : BaseConfiguration
    {
        private const int InvalidResult = -1;
        private static readonly GameDetailsRequiresResetComparer GameDetailsRequiresResetComparer = new();

        private readonly IBingoPaytableInstaller _bingoConfigurationProvider;

        public MachineAndGameConfiguration(
            IPropertiesManager propertiesManager,
            ISystemDisableManager systemDisableManager,
            IBingoPaytableInstaller bingoConfigurationProvider,
            IEventBus eventBus)
            : base(propertiesManager, systemDisableManager, eventBus)
        {
            _bingoConfigurationProvider = bingoConfigurationProvider ?? throw new ArgumentNullException(nameof(bingoConfigurationProvider));
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
                    { MachineAndGameConfigurationConstants.SideBetEnabled, (BingoConstants.SideBetEnabled, null)},
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
                case MachineAndGameConfigurationConstants.GamesConfigured when string.IsNullOrEmpty(model.ServerGameConfiguration):
                    model.ServerGameConfiguration = value;
                    var configured = JsonConvert.DeserializeObject<List<ServerGameConfiguration>>(value);
                    var results = _bingoConfigurationProvider.ConfigureGames(configured).ToList();
                    model.GamesConfigurationText = JsonConvert.SerializeObject(results);
                    break;
                case MachineAndGameConfigurationConstants.GamesConfigured:
                    model.ServerGameConfiguration = value;
                    var serverSettings = JsonConvert.DeserializeObject<List<ServerGameConfiguration>>(value);
                    var updateConfiguration = _bingoConfigurationProvider.UpdateConfiguration(serverSettings);
                    model.GamesConfigurationText = JsonConvert.SerializeObject(updateConfiguration);
                    break;
                default:
                    LogUnhandledSetting(name, value);
                    break;
            }
        }

        protected override void ServerConfigurationCompletedEvent()
        {
            EventBus.Publish(new ServerConfigurationCompletedEvent());
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
                _ => false,
            };
        }

        protected override bool SettingChangedThatRequiresNvRamClear(string name, string value, BingoServerSettingsModel model)
        {
            return name switch
            {
                MachineAndGameConfigurationConstants.GamesConfigured =>
                    !string.IsNullOrEmpty(model.ServerGameConfiguration) &&
                    GameSettingsRequireNvramChange(value, model.ServerGameConfiguration),
                _ => false
            };
        }

        private static bool GameSettingsRequireNvramChange(
            string serverSettings,
            string currentSettings)
        {
            var newSettings = JsonConvert.DeserializeObject<List<ServerGameConfiguration>>(serverSettings);
            var oldSettings = JsonConvert.DeserializeObject<List<ServerGameConfiguration>>(currentSettings);
            return newSettings.Count != oldSettings.Count ||
                   newSettings.Any(r => oldSettings.FirstOrDefault(c => GameSettingsMatch(r, c)) is null);
        }

        private static bool GameSettingsMatch(ServerGameConfiguration newSetting, ServerGameConfiguration oldSetting)
        {
            return GameDetailsRequiresResetComparer.Equals(newSetting, oldSetting);
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
                return _bingoConfigurationProvider.IsConfigurationValid(result);
            }
            catch (Exception ex)
            {
                Logger.Warn("Failed parsing the configuration data", ex);
                return false;
            }
        }
    }
}
