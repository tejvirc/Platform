namespace Aristocrat.Monaco.Bingo.Services.Configuration
{
    using System;
    using System.Collections.Generic;
    using Common;
    using Common.Storage.Model;
    using Gaming.Contracts;
    using Kernel;

    public class ComplianceConfiguration : BaseConfiguration
    {
        public ComplianceConfiguration(
            IPropertiesManager propertiesManager,
            ISystemDisableManager systemDisableManager)
            : base(propertiesManager, systemDisableManager)
        {
            ConfigurationConversion =
                new Dictionary<string, (string, Func<string, object>)>
                {
                    { ComplianceConfigurationConstants.WaitForPlayersLength, ("", val => Convert.ToInt64(val)) },
                    {
                        ComplianceConfigurationConstants.PlayerMayHideBingoCard,
                        (BingoConstants.PlayerMayHideBingoCard, null)
                    },
                    {
                        ComplianceConfigurationConstants.DispBingoCard,
                        (BingoConstants.DisplayBingoCardGlobal, null)
                    },
                    {
                       ComplianceConfigurationConstants.ReadySetGoMode,
                       (GamingConstants.ContinuousPlayMode, val => (ContinuousPlayMode)Enum.Parse(typeof(ContinuousPlayMode), val) ==
                       ContinuousPlayMode.PlayButtonOnePressNoRepeat ? PlayMode.Toggle : PlayMode.Continuous)
                    }
                };

            RequiredSettings =
                new HashSet<string>
                {
                    ComplianceConfigurationConstants.GameEndingPrize,
                    ComplianceConfigurationConstants.BingoType,
                    ComplianceConfigurationConstants.ReadySetGoMode
                };
        }

        protected override void AdditionalConfiguration(BingoServerSettingsModel model, string name, string value)
        {
            switch (name)
            {
                case ComplianceConfigurationConstants.PlayerMayHideBingoCard:
                    model.PlayerMayHideBingoCard = value;
                    break;
                case ComplianceConfigurationConstants.BallCallService:
                    model.BallCallService = value;
                    break;
                case ComplianceConfigurationConstants.BingoType:
                    model.BingoType = (BingoType)Enum.Parse(typeof(BingoType), value);
                    break;
                case ComplianceConfigurationConstants.GameEndingPrize:
                    model.GameEndingPrize = (GameEndWinStrategy)Enum.Parse(typeof(GameEndWinStrategy), value);
                    break;
                case ComplianceConfigurationConstants.DispBingoCard:
                    model.DisplayBingoCard = StringToBool(value);
                    break;
                case ComplianceConfigurationConstants.HideBingoCardWhenInactive:
                    model.HideBingoCardWhenInactive = StringToBool(value);
                    break;
                case ComplianceConfigurationConstants.ReadySetGoMode:
                    model.ReadySetGo = (ContinuousPlayMode)Enum.Parse(typeof(ContinuousPlayMode), value);
                    break;
                case ComplianceConfigurationConstants.WaitForPlayersLength:
                    model.WaitingForPlayersMs = long.Parse(value);
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
                ComplianceConfigurationConstants.WaitForPlayersLength => // must be between 1-14000
                    !long.TryParse(value, out var time) ||
                    time is > ComplianceConfigurationConstants.MaxWaitForPlayersMilliseconds or
                        <= ComplianceConfigurationConstants.MinWaitForPlayersMilliseconds,
                ComplianceConfigurationConstants.BallCallService => // only one value supported
                    !string.Equals(
                        value,
                        ComplianceConfigurationConstants.BallCallServiceLC2003,
                        StringComparison.Ordinal),
                ComplianceConfigurationConstants.PlayerMayHideBingoCard => !IsBooleanValue(value),
                ComplianceConfigurationConstants.DispBingoCard => !IsBooleanValue(value),
                ComplianceConfigurationConstants.HideBingoCardWhenInactive => !IsBooleanValue(value),
                ComplianceConfigurationConstants.BingoType =>
                    !Enum.TryParse<BingoType>(value, out var bingoType) ||
                    bingoType is <= BingoType.Unknown or >= BingoType.MaxBingoType,
                ComplianceConfigurationConstants.GameEndingPrize =>
                    !Enum.TryParse<GameEndWinStrategy>(value, out var gameEndWinStrategy) ||
                    gameEndWinStrategy != GameEndWinStrategy.BonusCredits,
                //We only support OnePressNoRepeat and PlayButtonOnePress
                ComplianceConfigurationConstants.ReadySetGoMode =>
                    !Enum.TryParse<ContinuousPlayMode>(value, out var mode) ||
                    (mode != ContinuousPlayMode.PlayButtonOnePressNoRepeat &&
                    mode != ContinuousPlayMode.PlayButtonOnePress),
                _ => false
            };
        }

        protected override bool SettingChangedThatRequiresNvRamClear(string name, string value, BingoServerSettingsModel model)
        {
            return name switch
            {
                ComplianceConfigurationConstants.BingoType => SettingChanged(model.BingoType, (BingoType)Enum.Parse(typeof(BingoType), value)),
                ComplianceConfigurationConstants.BallCallService => SettingChanged(model.BallCallService, value),
                _ => false
            };
        }
    }
}