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

    public sealed class SystemConfiguration : BaseConfiguration
    {
        private static readonly IReadOnlyDictionary<string, Func<string, bool>> InvalidSettingsHandlers =
            new Dictionary<string, Func<string, bool>>
            {
                {
                    SystemConfigurationConstants.MinJackpotValue,
                    value => !long.TryParse(value, out var result) || result <= 0
                },
                {
                    SystemConfigurationConstants.MinHandpayValue,
                    value => !long.TryParse(value, out var result) || result <= 0
                },
                {
                    SystemConfigurationConstants.TransferLimit,
                    value => !long.TryParse(value, out var result) || result < 0
                },
                {
                    SystemConfigurationConstants.VoucherThreshold,
                    value => !long.TryParse(value, out var result) || result < 0
                },
                {
                    SystemConfigurationConstants.Gen8MaxVoucherIn,
                    value => !long.TryParse(value, out var result) || result < 0
                },
                {
                    SystemConfigurationConstants.BadCountThreshold,
                    value => !int.TryParse(value, out var result) || result < 0
                },
                {
                    SystemConfigurationConstants.JackpotHandlingStrategy,
                    value => !Enum.TryParse<JackpotStrategy>(value, out var jackpotStrategy) ||
                             jackpotStrategy is <= JackpotStrategy.CreditJackpotWin
                                 or >= JackpotStrategy.MaxJackpotStrategy
                },
                {
                    SystemConfigurationConstants.JackpotAmountDetermination,
                    value => !Enum.TryParse<JackpotDetermination>(value, out var jackpotStrategy) ||
                             jackpotStrategy is <= JackpotDetermination.Unknown
                                 or >= JackpotDetermination.MaxJackpotDetermination
                },
                {
                    SystemConfigurationConstants.CreditsStrategy,
                    value => !Enum.TryParse<CreditsStrategy>(value, out var creditsStrategy) ||
                             creditsStrategy != CreditsStrategy.Sas
                },
                { SystemConfigurationConstants.AudibleAlarmSetting, value => !IsBooleanValue(value) },
                { SystemConfigurationConstants.TicketReprint, value => !IsBooleanValue(value) },
                { SystemConfigurationConstants.HandpayReceipt, value => !IsBooleanValue(value) },
                { SystemConfigurationConstants.AftBonusing, value => !IsBooleanValue(value) },
                { SystemConfigurationConstants.SasAft, value => !IsBooleanValue(value) },
                { SystemConfigurationConstants.RecordGamePlay, value => !IsBooleanValue(value) },
                { SystemConfigurationConstants.CashOutButton, value => !IsBooleanValue(value) },
                { SystemConfigurationConstants.TransferWin2Host, value => !IsBooleanValue(value) }
            };

        private readonly IReadOnlyDictionary<string, Action<BingoServerSettingsModel, IPropertiesManager, string>>
            _additionalConfigurationHandler = new Dictionary<string, Action<BingoServerSettingsModel, IPropertiesManager, string>>
            {
                { SystemConfigurationConstants.Gen8MaxVoucherIn, (model, _, value) => model.VoucherInLimit = long.Parse(value) },
                { SystemConfigurationConstants.HandpayReceipt, (model, _, value) => model.PrintHandpayReceipt = StringToBool(value) },
                { SystemConfigurationConstants.TicketReprint, (model, _, value) => model.TicketReprint = StringToBool(value) },
                { SystemConfigurationConstants.Gen8MaxCashIn, (model, _, value) => model.BillAcceptanceLimit = long.Parse(value) },
                { SystemConfigurationConstants.VoucherThreshold, (model, _, value) => model.MaximumVoucherValue = long.Parse(value) },
                { SystemConfigurationConstants.MinJackpotValue, HandleMinJackpotValue },
                { SystemConfigurationConstants.AudibleAlarmSetting, (model, _, value) => model.AlarmConfiguration = StringToBool(value) },
                { SystemConfigurationConstants.RecordGamePlay, (model, _, value) => model.CaptureGameAnalytics = StringToBool(value) },
                { SystemConfigurationConstants.CaptureGameAnalytics, (model, _, value) => model.CaptureGameAnalytics = StringToBool(value) },
                { SystemConfigurationConstants.JackpotHandlingStrategy, HandleJackpotHandlingStrategy },
                { SystemConfigurationConstants.CreditsStrategy, (model, _, value) => model.CreditsStrategy = value.ToEnumeration<CreditsStrategy>() },
                { SystemConfigurationConstants.JackpotAmountDetermination, (model, _, value) => model.JackpotAmountDetermination = value.ToEnumeration<JackpotDetermination>() },
                { SystemConfigurationConstants.AftBonusing, (model, _, value) => model.AftBonusingEnabled = StringToBool(value) },
                { SystemConfigurationConstants.SasLegacyBonusing, (model, _, value) => model.LegacyBonusAllowed = value }
            };

        public SystemConfiguration(
            IPropertiesManager propertiesManager,
            IMultiProtocolConfigurationProvider protocolConfiguration,
            ISystemDisableManager systemDisableManager,
            IEventBus eventBus)
            : base(propertiesManager, systemDisableManager, eventBus)
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
                            val => SetSasFeature(x => x.AftBonusAllowed = StringToBool(val)))
                    },
                    {
                        SystemConfigurationConstants.SasLegacyBonusing, (SasProperties.SasFeatureSettings,
                            val => SetSasFeature(x => x.LegacyBonusAllowed = StringToBool(val)))
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
                RequiredSettings.RemoveWhere(x => x.Equals(SystemConfigurationConstants.AftBonusing, StringComparison.Ordinal) ||
                    x.Equals(SystemConfigurationConstants.SasLegacyBonusing, StringComparison.Ordinal));
            }
        }

        protected override void AdditionalConfiguration(BingoServerSettingsModel model, string name, string value)
        {
            if (!_additionalConfigurationHandler.TryGetValue(name, out var handler))
            {
                LogUnhandledSetting(name, value);
                return;
            }

            handler(model, PropertiesManager, value);
        }

        protected override bool IsSettingInvalid(string name, string value)
        {
            return InvalidSettingsHandlers.TryGetValue(name, out var handler) && handler(value);
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

        private static void HandleJackpotHandlingStrategy(
            BingoServerSettingsModel model,
            IPropertiesManager propertiesManager,
            string value)
        {
            model.JackpotStrategy = value.ToEnumeration<JackpotStrategy>();
            propertiesManager.ConfigureLargeWinStrategy(model.JackpotStrategy);
        }

        private SasFeatures SetSasFeature(Action<SasFeatures> action)
        {
            var features = (SasFeatures)PropertiesManager.GetValue(
                SasProperties.SasFeatureSettings,
                new SasFeatures()).Clone();
            action.Invoke(features);
            return features;
        }

        private static void HandleMinJackpotValue(
            BingoServerSettingsModel model,
            IPropertiesManager propertiesManager,
            string value)
        {
            model.MinimumJackpotValue = long.Parse(value);
            var handpayLimit = (long)propertiesManager.GetProperty(AccountingConstants.HandpayLimit, long.MinValue);
            var jackpotLimitInMillicents = model.MinimumJackpotValue.Value.CentsToMillicents();
            if (jackpotLimitInMillicents > handpayLimit)
            {
                propertiesManager.SetProperty(AccountingConstants.HandpayLimit, jackpotLimitInMillicents);
            }
        }
    }
}
