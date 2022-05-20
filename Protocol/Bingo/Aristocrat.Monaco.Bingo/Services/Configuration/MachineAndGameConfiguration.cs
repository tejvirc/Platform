namespace Aristocrat.Monaco.Bingo.Services.Configuration
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using Common;
    using Common.Storage.Model;
    using Gaming.Contracts;
    using Kernel;
    using Quartz.Util;

    public class MachineAndGameConfiguration : BaseConfiguration
    {
        private const int InvalidResult = -1;

        public MachineAndGameConfiguration(
            IPropertiesManager propertiesManager,
            ISystemDisableManager systemDisableManager)
            : base(propertiesManager, systemDisableManager)
        {
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
                    { MachineAndGameConfigurationConstants.QuickStopMode, (GamingConstants.ReelStopEnabled, val => StringToBool(val))},
                    { MachineAndGameConfigurationConstants.BingoHelpUri, (BingoConstants.BingoHelpUri, null)}
                };

            RequiredSettings =
                new HashSet<string>
                {
                    MachineAndGameConfigurationConstants.GameTitleId
                };
        }

        protected override void AdditionalConfiguration(BingoServerSettingsModel model, string name, string value)
        {
            switch (name)
            {
                case MachineAndGameConfigurationConstants.LocationZoneId:
                    model.ZoneId = value;
                    break;
                case MachineAndGameConfigurationConstants.GameTitleId:
                    model.GameTitles = value;
                    break;
                case MachineAndGameConfigurationConstants.BonusGame:
                    model.BonusGames = value;
                    break;
                case MachineAndGameConfigurationConstants.EvaluationTypePaytable:
                    model.EvaluationTypePaytable = (PaytableEvaluation)Enum.Parse(typeof(PaytableEvaluation), value);
                    break;
                case MachineAndGameConfigurationConstants.ThemeSkin:
                    model.ThemeSkins = value;
                    break;
                case MachineAndGameConfigurationConstants.PaytableId:
                    model.PaytableIds = value;
                    break;
                case MachineAndGameConfigurationConstants.LocationBank:
                    model.BankId = value;
                    break;
                case MachineAndGameConfigurationConstants.LocationPosition:
                    model.Position = value;
                    break;
                case MachineAndGameConfigurationConstants.QuickStopMode:
                    model.QuickStopMode = StringToBool(value);
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
                default:
                    LogUnhandledSetting(name, value);
                    break;
            }
        }

        protected override bool IsSettingInvalid(string name, string value)
        {
            return name switch
            {
                // must be > 0
                MachineAndGameConfigurationConstants.MachineSerial => (long.TryParse(value, out var result) ? result : InvalidResult) <= 0,
                // must be >= 0
                MachineAndGameConfigurationConstants.LocationId => (int.TryParse(value, out var result1) ? result1 : InvalidResult) < 0,
                // must be >= 0
                MachineAndGameConfigurationConstants.MachineTypeId => (int.TryParse(value, out var result2) ? result2 : InvalidResult) < 0,
                // must be >= 0
                MachineAndGameConfigurationConstants.CreditsManager => (int.TryParse(value, out var result3) ? result3 : InvalidResult) < 0,
                // must be >= 1
                MachineAndGameConfigurationConstants.NumGamesConfigured => (int.TryParse(value, out var result4) ? result4 : InvalidResult) < 1,
                // must be >= 0
                MachineAndGameConfigurationConstants.GameTitleId => (int.TryParse(value, out var result5) ? result5 : InvalidResult) < 0,
                // must be >= 0
                MachineAndGameConfigurationConstants.ThemeSkin => (int.TryParse(value, out var result6) ? result6 : InvalidResult) < 0,
                // must be >= 0
                MachineAndGameConfigurationConstants.PaytableId => (int.TryParse(value, out var result7) ? result7 : InvalidResult) < 0,
                // must be >= 0
                MachineAndGameConfigurationConstants.DenominationId => (int.TryParse(value, out var result8) ? result8 : InvalidResult) < 0,
                // must be valid bool
                MachineAndGameConfigurationConstants.QuickStopMode => !IsBooleanValue(value),
                // valid values are "EGM Setting" or "Top Screen"
                MachineAndGameConfigurationConstants.BingoCardPlacement => ValidateBingoCardPlacement(value),
                // valid values are "Use Global Settings" or any bool
                MachineAndGameConfigurationConstants.DispBingoCard =>
                    !(UsingGlobalSetting(value) || IsBooleanValue(value)),
                MachineAndGameConfigurationConstants.HideBingoCardWhenInactive =>
                    !(UsingGlobalSetting(value) || IsBooleanValue(value)),
                // must be valid PaytableEvaluation enum value
                MachineAndGameConfigurationConstants.EvaluationTypePaytable =>
                    !Enum.TryParse<PaytableEvaluation>(value, out var paytableEvaluation) ||
                    paytableEvaluation is <= PaytableEvaluation.Unknown or >= PaytableEvaluation.MaxPaytableMethod,
                MachineAndGameConfigurationConstants.BingoHelpUri =>
                    value.IsNullOrWhiteSpace() || !Uri.IsWellFormedUriString(value, UriKind.RelativeOrAbsolute),
                _ => false,
            };
        }

        protected override bool SettingChangedThatRequiresNvRamClear(string name, string value, BingoServerSettingsModel model)
        {
            return name switch
            {
                MachineAndGameConfigurationConstants.GameTitleId => SettingChanged(model.GameTitles, value),
                MachineAndGameConfigurationConstants.BonusGame => SettingChanged(model.BonusGames, value),
                MachineAndGameConfigurationConstants.ThemeSkin => SettingChanged(model.ThemeSkins, value),
                MachineAndGameConfigurationConstants.QuickStopMode => SettingChanged(model.QuickStopMode, StringToBool(value)),
                MachineAndGameConfigurationConstants.PaytableId => SettingChanged(model.PaytableIds, value),
                MachineAndGameConfigurationConstants.EvaluationTypePaytable => SettingChanged(model.EvaluationTypePaytable,
                    (PaytableEvaluation)Enum.Parse(typeof(PaytableEvaluation), value)),
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
    }
}
