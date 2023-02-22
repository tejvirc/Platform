namespace Aristocrat.Monaco.Bingo.Services.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Accounting.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Protocol;
    using Common.Storage.Model;
    using Hardware.Contracts;
    using Kernel;
    using Kernel.Contracts;
    using Monaco.Common;
    using Sas.Contracts.SASProperties;

    public class SystemConfiguration : BaseConfiguration
    {
        public SystemConfiguration(
            IPropertiesManager propertiesManager,
            IMultiProtocolConfigurationProvider protocolConfiguration,
            ISystemDisableManager systemDisableManager)
            : base(propertiesManager, systemDisableManager)
        {
            if (protocolConfiguration == null)
            {
                throw new ArgumentNullException(nameof(protocolConfiguration));
            }

            ConfigurationConversion =
                new Dictionary<string, (string, Func<string, object>)>
                {
                    {
                        SystemConfigurationConstants.Gen8MaxVoucherIn,
                        (AccountingConstants.VoucherInLimit, val => long.Parse(val).CentsToMillicents())
                    },
                    {
                        SystemConfigurationConstants.MinJackpotValue,
                        (AccountingConstants.LargeWinLimit, val => long.Parse(val).CentsToMillicents())
                    },
                    {
                        SystemConfigurationConstants.HandpayReceipt,
                        (AccountingConstants.EnableReceipts, val => StringToBool(val))
                    },
                    {
                        SystemConfigurationConstants.TicketReprint,
                        (AccountingConstants.ReprintLoggedVoucherBehavior, val => StringToBool(val) ? "Last" : "None" )
                    },
                    {
                        SystemConfigurationConstants.Gen8MaxCashIn,
                        (PropertyKey.MaxCreditsIn, val => long.Parse(val).CentsToMillicents())
                    },
                    {
                        SystemConfigurationConstants.VoucherThreshold,
                        (AccountingConstants.VoucherOutLimit, val => long.Parse(val).CentsToMillicents())
                    },
                    {
                        SystemConfigurationConstants.AudibleAlarmSetting,
                        (HardwareConstants.DoorAlarmEnabledKey, val => StringToBool(val))
                    },
                    {
                        SystemConfigurationConstants.AftBonusing, (SasProperties.SasFeatureSettings,
                            val => SetSASFeature(x => x.AftBonusAllowed = StringToBool(val)))
                    },
                    {
                        SystemConfigurationConstants.SasLegacyBonusing, (SasProperties.SasFeatureSettings,
                            val => SetSASFeature(x => x.LegacyBonusAllowed = StringToBool(val)))
                    }
                };

            RequiredSettings =
                new HashSet<string>
                {
                    SystemConfigurationConstants.Gen8MaxVoucherIn,
                    SystemConfigurationConstants.HandpayReceipt,
                    SystemConfigurationConstants.TicketReprint,
                    SystemConfigurationConstants.Gen8MaxCashIn,
                    SystemConfigurationConstants.VoucherThreshold,
                    SystemConfigurationConstants.MinJackpotValue,
                    SystemConfigurationConstants.AftBonusing,
                    SystemConfigurationConstants.SasLegacyBonusing,
                    SystemConfigurationConstants.RecordGamePlay,
                    SystemConfigurationConstants.JackpotHandlingStrategy,
                    SystemConfigurationConstants.CreditsStrategy,
                    SystemConfigurationConstants.JackpotAmountDetermination
                };

            if (!protocolConfiguration.MultiProtocolConfiguration.Select(x => x.Protocol).Contains(CommsProtocol.SAS))
            {
                RequiredSettings.RemoveWhere(x => x.Equals(SystemConfigurationConstants.AftBonusing) ||
                    x.Equals(SystemConfigurationConstants.SasLegacyBonusing));
            }
        }

        protected override void AdditionalConfiguration(BingoServerSettingsModel model, string name, string value)
        {
            switch (name)
            {
                case SystemConfigurationConstants.Gen8MaxVoucherIn:
                    model.VoucherInLimit = long.Parse(value);
                    break;
                case SystemConfigurationConstants.HandpayReceipt:
                    model.PrintHandpayReceipt = StringToBool(value);
                    break;
                case SystemConfigurationConstants.TicketReprint:
                    model.TicketReprint = StringToBool(value);
                    break;
                case SystemConfigurationConstants.Gen8MaxCashIn:
                    model.BillAcceptanceLimit = long.Parse(value);
                    break;
                case SystemConfigurationConstants.VoucherThreshold:
                    model.MaximumVoucherValue = long.Parse(value);
                    break;
                case SystemConfigurationConstants.MinJackpotValue:
                    model.MinimumJackpotValue = long.Parse(value);
                    ValidateHandpayLimit(model.MinimumJackpotValue.Value);
                    break;
                case SystemConfigurationConstants.AudibleAlarmSetting:
                    model.AlarmConfiguration = StringToBool(value);
                    break;
                case SystemConfigurationConstants.RecordGamePlay:
                case SystemConfigurationConstants.CaptureGameAnalytics:
                    model.CaptureGameAnalytics = StringToBool(value);
                    break;
                case SystemConfigurationConstants.JackpotHandlingStrategy:
                    model.JackpotStrategy = value.ToEnumeration<JackpotStrategy>();
                    PropertiesManager.ConfigureLargeWinStrategy(model.JackpotStrategy);
                    break;
                case SystemConfigurationConstants.CreditsStrategy:
                    model.CreditsStrategy = value.ToEnumeration<CreditsStrategy>();
                    break;
                case SystemConfigurationConstants.JackpotAmountDetermination:
                    model.JackpotAmountDetermination = value.ToEnumeration<JackpotDetermination>();
                    break;
                case SystemConfigurationConstants.AftBonusing:
                    model.AftBonusingEnabled = StringToBool(value);
                    break;
                case SystemConfigurationConstants.SasLegacyBonusing:
                    model.LegacyBonusAllowed = value;
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
                SystemConfigurationConstants.MinJackpotValue => !long.TryParse(value, out var result) || result <= 0,
                SystemConfigurationConstants.MinHandpayValue => !long.TryParse(value, out var result) || result <= 0,
                SystemConfigurationConstants.TransferLimit => !long.TryParse(value, out var result) || result < 0,
                SystemConfigurationConstants.VoucherThreshold => !long.TryParse(value, out var result) || result < 0,
                SystemConfigurationConstants.Gen8MaxVoucherIn => !long.TryParse(value, out var result) || result < 0,
                SystemConfigurationConstants.BadCountThreshold => !int.TryParse(value, out var result) || result < 0,
                SystemConfigurationConstants.JackpotHandlingStrategy =>
                    !Enum.TryParse<JackpotStrategy>(value, out var jackpotStrategy) ||
                    jackpotStrategy is <= JackpotStrategy.CreditJackpotWin or >= JackpotStrategy.MaxJackpotStrategy,
                SystemConfigurationConstants.JackpotAmountDetermination =>
                    !Enum.TryParse<JackpotDetermination>(value, out var jackpotStrategy) ||
                    jackpotStrategy is <= JackpotDetermination.Unknown or >= JackpotDetermination.MaxJackpotDetermination,
                SystemConfigurationConstants.CreditsStrategy =>
                    !Enum.TryParse<CreditsStrategy>(value, out var creditsStrategy) ||
                    creditsStrategy != CreditsStrategy.Sas,
                SystemConfigurationConstants.AudibleAlarmSetting => !IsBooleanValue(value),
                SystemConfigurationConstants.TicketReprint => !IsBooleanValue(value),
                SystemConfigurationConstants.HandpayReceipt => !IsBooleanValue(value),
                SystemConfigurationConstants.AftBonusing => !IsBooleanValue(value),
                SystemConfigurationConstants.SasAft => !IsBooleanValue(value),
                SystemConfigurationConstants.RecordGamePlay => !IsBooleanValue(value),
                SystemConfigurationConstants.CashOutButton => !IsBooleanValue(value),
                SystemConfigurationConstants.TransferWin2Host => !IsBooleanValue(value),
                _ => false
            };
        }

        protected override bool SettingChangedThatRequiresNvRamClear(string name, string value, BingoServerSettingsModel model)
        {
            return name switch
            {
                // These should match Appendix C of the Application Management Console User Manual.
                SystemConfigurationConstants.VoucherThreshold => SettingChanged(
                    model.MaximumVoucherValue,
                    long.Parse(value)),
                SystemConfigurationConstants.JackpotHandlingStrategy => SettingChanged(
                    model.JackpotStrategy,
                    value.ToEnumeration<JackpotStrategy>()),
                SystemConfigurationConstants.MinJackpotValue => SettingChanged(
                    model.MinimumJackpotValue,
                    long.Parse(value)),
                // SAS Type and Backend Type are not sent from server; they are determined during EGM configuration (after NVRAM Clear!).
                SystemConfigurationConstants.SasLegacyBonusing => SettingChanged(
                    model.LegacyBonusAllowed,
                    value),
                _ => false
            };
        }

        private SasFeatures SetSASFeature(Action<SasFeatures> action)
        {
            var features = (SasFeatures)PropertiesManager.GetValue(
                SasProperties.SasFeatureSettings,
                new SasFeatures()).Clone();
            action.Invoke(features);
            return features;
        }

        private void ValidateHandpayLimit(long minimumJackpotValue)
        {
            var handpayLimit = (long)PropertiesManager.GetProperty(AccountingConstants.HandpayLimit, long.MinValue);
            var jackpotLimitInMillicents = minimumJackpotValue.CentsToMillicents();
            if (jackpotLimitInMillicents > handpayLimit)
            {
                PropertiesManager.SetProperty(AccountingConstants.HandpayLimit, jackpotLimitInMillicents);
            }
        }
    }
}
