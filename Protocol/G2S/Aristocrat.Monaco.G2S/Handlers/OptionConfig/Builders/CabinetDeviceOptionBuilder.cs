namespace Aristocrat.Monaco.G2S.Handlers.OptionConfig.Builders
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using Data.OptionConfig;
    using Gaming.Contracts;
    using Kernel;
    using Kernel.Contracts;
    using Linq;
    using Monaco.Common;

    /// <inheritdoc />
    public class CabinetDeviceOptionBuilder : BaseDeviceOptionBuilder<CabinetDevice>
    {
        private const string DefaultCulture = @"en-US";
        private const string DefaultIdleText = @"INSERT MONEY TO PLAY";

        private readonly ILocalization _locale;
        private readonly IPropertiesManager _properties;

        public CabinetDeviceOptionBuilder(IPropertiesManager properties, ILocalization locale)
        {
            _properties = properties;
            _locale = locale;
        }

        /// <inheritdoc />
        protected override DeviceClass DeviceClass => DeviceClass.Cabinet;

        /// <inheritdoc />
        protected override optionGroup[] BuildGroups(
            CabinetDevice device,
            OptionListCommandBuilderParameters parameters)
        {
            var optionGroups = new List<optionGroup>();

            var group = new optionGroup
            {
                optionGroupId = OptionConstants.CabinetProfileOptionsId, optionGroupName = "G2S Cabinet Options"
            };

            var items = new List<optionItem>();

            if (ShouldIncludeParam(OptionConstants.ProtocolOptionsId, parameters))
            {
                items.Add(
                    BuildProtocolOptions(
                        device,
                        device.RestartStatus,
                        "Standard G2S protocol parameters for cabinet devices",
                        parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.CabinetProfileOptionsId, parameters))
            {
                items.Add(BuildCabinetProfileOptions(device, parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.CabinetLimitOptionsId, parameters))
            {
                items.Add(BuildCabinetLimitOptions(parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.ProtocolAdditionalOptionsId, parameters))
            {
                items.Add(BuildProtocolAdditionalOptions(device, parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.CabinetRemoteActionsOptionsId, parameters))
            {
                items.Add(BuildCabinetRemoteActionsOptions(parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.ConfigurationDelayPeriodOptionsId, parameters))
            {
                items.Add(BuildConfigurationDelayPeriodOptions(device, parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.CashOutOnDisableOptionsId, parameters))
            {
                items.Add(BuildCashOutOnDisableOptions(parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.FaultsSupportedOptionsId, parameters))
            {
                items.Add(BuildFaultsSupportedOptions(parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.TimeZonesSupportedOptionsId, parameters))
            {
                items.Add(BuildTimeZonesSupportedOptions(parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.PropertyIdentifierOptionsId, parameters))
            {
                items.Add(BuildPropertyIdentifierOptions(parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.GameLimitsOptionsId, parameters))
            {
                items.Add(BuildGameLimitsOptions(parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.OccupancyMeterOptionsId, parameters))
            {
                items.Add(BuildOccupancyMeterOptions(parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.GtechProtocolOptionsId, parameters))
            {
                items.Add(BuildGtechProtocolOptions(device, parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.ReelStopOptionId, parameters))
            {
                items.Add(BuildReelStopOptions(parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.ReelDurationOptionId, parameters))
            {
                items.Add(BuildReelDurationOptions(parameters.IncludeDetails));
            }

            // GTECH/Spielo
            if (ShouldIncludeParam(OptionConstants.IdleTextOptionId, parameters))
            {
                items.Add(BuildIdleTextOptions(parameters.IncludeDetails));
            }

            group.optionItem = items.ToArray();
            optionGroups.Add(group);

            // Quebec G2S Phase 2.1
            if (ShouldIncludeParam(OptionConstants.BankCashoutOptionId, parameters))
            {
                optionGroups.Add(
                    new optionGroup
                    {
                        optionGroupId = "IGT_bankCashOut",
                        optionItem = new[] { BuildBankCashoutOptions(parameters.IncludeDetails) }
                    });
            }

            var igtCabinetLimits = new optionGroup { optionGroupId = "IGT_cabinetLimits" };

            var igtCabinetLimitItems = new List<optionItem>();

            // Quebec G2S Phase 2.1
            if (ShouldIncludeParam(OptionConstants.CashInLimitOptionId, parameters))
            {
                igtCabinetLimitItems.Add(BuildCashInLimitOptions(parameters.IncludeDetails));
            }

            // Quebec G2S Phase 2.1
            if (ShouldIncludeParam(OptionConstants.NonCashInLimitOptionId, parameters))
            {
                igtCabinetLimitItems.Add(BuildNonCashInLimitOptions(parameters.IncludeDetails));
            }

            igtCabinetLimits.optionItem = igtCabinetLimitItems.ToArray();
            optionGroups.Add(igtCabinetLimits);

            return optionGroups.ToArray();
        }

        private optionItem BuildCabinetProfileOptions(IDevice device, bool includeDetails)
        {
            var parameters = new[]
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.CabinetDevice.MachineNumberParameterName,
                    ParamName = "Machine Number",
                    ParamHelp = "Property assigned machine number",
                    ParamCreator =
                        () => new integerParameter
                        {
                            canModRemote = true, canModLocal = true, minIncl = 0, maxIncl = int.MaxValue
                        },
                    ValueCreator = () => new integerValue1(),
                    DefaultValue = 0,
                    Value = (int)_properties.GetValue(ApplicationConstants.MachineId, (uint)0)
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.CabinetDevice.MachineIdParameterName,
                    ParamName = "Machine Id",
                    ParamHelp = "Property assigned alphanumeric machine identifier",
                    ParamCreator = () =>
                        new stringParameter { canModRemote = true, canModLocal = true, minLen = 0, maxLen = 8 },
                    ValueCreator = () => new stringValue1(),
                    DefaultValue = string.Empty,
                    Value = _properties.GetValue(ApplicationConstants.SerialNumber, string.Empty)
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.CurrencyIdParameterName,
                    ParamName = "Base Currency",
                    ParamHelp = "Base currency of the EGM",
                    ParamCreator = () => new stringParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new stringValue1(),
                    DefaultValue = new RegionInfo(DefaultCulture).ISOCurrencySymbol,
                    Value = _properties.GetValue(
                        ApplicationConstants.CurrencyId,
                        _locale.RegionInfo.ISOCurrencySymbol)
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.CabinetDevice.ReportDenomIdParameterName,
                    ParamName = "Base Denomination",
                    ParamHelp = "Property assigned denomination used for reporting",
                    ParamCreator = () => new integerParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new integerValue1(),
                    DefaultValue = G2S.Constants.DefaultReportDenomId,
                    Value = _properties.GetValue(G2S.Constants.ReportDenomId, G2S.Constants.DefaultReportDenomId)
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.CabinetDevice.LocaleIdParameterName,
                    ParamName = "Locale",
                    ParamHelp = "Locale (language_country) of the EGM",
                    ParamCreator = () => new stringParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new stringValue1(),
                    DefaultValue = device.LocaleId(CultureInfo.GetCultureInfo(DefaultCulture)),
                    Value = device.LocaleId(_locale.CurrentCulture)
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.CabinetDevice.AreaIdParameterName,
                    ParamName = "Area",
                    ParamHelp = "Property assigned area identifier",
                    ParamCreator = () =>
                        new stringParameter { canModRemote = true, canModLocal = true, minLen = 0, maxLen = 8 },
                    ValueCreator = () => new stringValue1(),
                    DefaultValue = string.Empty,
                    Value = _properties.GetValue(ApplicationConstants.Area, string.Empty)
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.CabinetDevice.ZoneIdParameterName,
                    ParamName = "Zone",
                    ParamHelp = "Property assigned zone identifier",
                    ParamCreator = () =>
                        new stringParameter { canModRemote = true, canModLocal = true, minLen = 0, maxLen = 8 },
                    ValueCreator = () => new stringValue1(),
                    DefaultValue = string.Empty,
                    Value = _properties.GetValue(ApplicationConstants.Zone, string.Empty)
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.CabinetDevice.BankIdParameterName,
                    ParamName = "Bank",
                    ParamHelp = "Property assigned bank identifier",
                    ParamCreator = () =>
                        new stringParameter { canModRemote = true, canModLocal = true, minLen = 0, maxLen = 8 },
                    ValueCreator = () => new stringValue1(),
                    DefaultValue = string.Empty,
                    Value = _properties.GetValue(ApplicationConstants.Bank, string.Empty)
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.CabinetDevice.EgmPositionParameterName,
                    ParamName = "Position",
                    ParamHelp = "EGM location within the bank",
                    ParamCreator = () =>
                        new stringParameter { canModRemote = true, canModLocal = true, minLen = 0, maxLen = 8 },
                    ValueCreator = () => new stringValue1(),
                    DefaultValue = string.Empty,
                    Value = _properties.GetValue(ApplicationConstants.Position, string.Empty)
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.CabinetDevice.MachineLocationParameterName,
                    ParamName = "Location",
                    ParamHelp = "EGM physical location identifier",
                    ParamCreator = () =>
                        new stringParameter { canModRemote = true, canModLocal = true, minLen = 0, maxLen = 16 },
                    ValueCreator = () => new stringValue1(),
                    DefaultValue = string.Empty,
                    Value = _properties.GetValue(ApplicationConstants.Location, string.Empty)
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.CabinetDevice.CabinetStyleParameterName,
                    ParamName = "Cabinet Style",
                    ParamHelp = "EGM Cabinet Style",
                    ParamCreator = () => new stringParameter(),
                    ValueCreator = () => new stringValue1(),
                    DefaultValue = G2S.Constants.DefaultCabinetStyle,
                    Value = _properties.GetValue(G2S.Constants.CabinetStyle, G2S.Constants.DefaultCabinetStyle)
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.CabinetDevice.IdleTimePeriodParameterName,
                    ParamName = "Idle Time Period",
                    ParamHelp = "Timeout with 0 credits when EGM is typically considered idle",
                    ParamCreator = () => new integerParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new integerValue1(),
                    DefaultValue = (int)GamingConstants.DefaultIdleTimeoutPeriod.TotalMilliseconds,
                    Value = _properties.GetValue(
                        GamingConstants.IdleTimePeriod,
                        (int)GamingConstants.DefaultIdleTimeoutPeriod.TotalMilliseconds)
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.CabinetDevice.TimeZoneOffsetParameterName,
                    ParamName = "Time Zone Offset",
                    ParamHelp = "Time zone offset from UCT for this EGM",
                    ParamCreator = () => new stringParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new stringValue1(),
                    DefaultValue = TimeSpan.Zero.GetFormattedOffset(),
                    Value = _properties.GetValue(ApplicationConstants.TimeZoneOffsetKey, TimeSpan.Zero)
                        .GetFormattedOffset()
                }
            };

            return BuildOptionItem(
                OptionConstants.CabinetProfileOptionsId,
                t_securityLevels.G2S_attendant,
                1,
                1,
                "G2S_cabinetParams",
                "Cabinet Parameters",
                "Casino specific parameters for the EGM",
                parameters,
                includeDetails);
        }

        private optionItem BuildCabinetLimitOptions(bool includeDetails)
        {
            var largeWinLimit = new ParameterDescription
            {
                ParamId = G2SParametersNames.CabinetDevice.LargeWinLimitParameterName,
                ParamName = "Large Win Limit",
                ParamHelp =
                    @"Maximum win that can be automatically paid by the EGM without generating a handpay request",
                ParamCreator = () => new integerParameter { canModRemote = true, canModLocal = true },
                ValueCreator = () => new integerValue1(),
                DefaultValue = AccountingConstants.DefaultLargeWinLimit,
                Value = (long)_properties.GetProperty(
                    AccountingConstants.LargeWinLimit,
                    AccountingConstants.DefaultLargeWinLimit)
            };
            var creditMeterMax = new ParameterDescription
            {
                ParamId = G2SParametersNames.CabinetDevice.MaxCreditMeterParameterName,
                ParamName = "Credit Meter Maximum",
                ParamHelp = "Maximum amount permitted on the credit meter",
                ParamCreator = () => new integerParameter { canModRemote = true, canModLocal = true },
                ValueCreator = () => new integerValue1(),
                DefaultValue = G2S.Constants.DefaultMaxCreditMeter,
                Value = (long)_properties.GetProperty(
                    AccountingConstants.MaxCreditMeter,
                    G2S.Constants.DefaultMaxCreditMeter)
            };
            var maxHopperPayout = new ParameterDescription
            {
                ParamId = G2SParametersNames.CabinetDevice.MaxHopperPayOutParameterName,
                ParamName = "Max Hopper Payout",
                ParamHelp = "Maximum amount to pay from the hopper",
                ParamCreator = () => new integerParameter { canModRemote = true, canModLocal = true },
                ValueCreator = () => new integerValue1(),
                DefaultValue = 0,
                Value = 0
            };
            var okToSplit = new ParameterDescription
            {
                ParamId = G2SParametersNames.CabinetDevice.SplitPayOutParameterName,
                ParamName = "OK to Split Payouts",
                ParamHelp = "Split large payouts between hopper and other means",
                ParamCreator = () => new booleanParameter { canModRemote = true, canModLocal = true },
                ValueCreator = () => new booleanValue1(),
                DefaultValue = false,
                Value = true
            };
            var acceptNonCash = new ParameterDescription
            {
                ParamId = G2SParametersNames.CabinetDevice.AcceptNonCashableMoneyParameterName,
                ParamName = "Accept Non-Cashable Money",
                ParamHelp = "Non-cashable transfers can be accepted by this EGM",
                ParamCreator = () => new stringParameter { canModRemote = true, canModLocal = true },
                ValueCreator = () => new stringValue1(),
                DefaultValue = t_acceptNonCashAmts.G2S_acceptAlways.ToName(),
                Value = t_acceptNonCashAmts.G2S_acceptAlways.ToName()
            };

            var parameters = new[] { largeWinLimit, creditMeterMax, maxHopperPayout, okToSplit, acceptNonCash };

            return BuildOptionItem(
                OptionConstants.CabinetLimitOptionsId,
                t_securityLevels.G2S_operator,
                1,
                1,
                "G2S_limitParams",
                "Cabinet Limits",
                "Settable Cabinet Limits for the EGM",
                parameters,
                includeDetails);
        }

        private optionItem BuildCabinetRemoteActionsOptions(bool includeDetails)
        {
            var parameters = new[]
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.CabinetDevice.G2SResetSupportedParameterName,
                    ParamName = "Remote Reset OK",
                    ParamHelp = "EGM Supports Remote Reset",
                    ParamCreator = () => new booleanParameter(),
                    ValueCreator = () => new booleanValue1(),
                    Value = false,
                    DefaultValue = false
                }
            };

            return BuildOptionItem(
                OptionConstants.CabinetRemoteActionsOptionsId,
                t_securityLevels.G2S_operator,
                1,
                1,
                "G2S_remoteActionParams",
                "Remote Action Support",
                "Which Remote Actions Are Supported",
                parameters,
                includeDetails);
        }

        private optionItem BuildConfigurationDelayPeriodOptions(ICabinetDevice device, bool includeDetails)
        {
            var parameters = new[]
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.CabinetDevice.ConfigDelayPeriodParameterName,
                    ParamName = "Configuration Delay Period",
                    ParamHelp = "Minimum configuration delay period",
                    ParamCreator = () => new integerParameter(),
                    ValueCreator = () => new integerValue1(),
                    DefaultValue = 240000,
                    Value = device.ConfigDelayPeriod
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.CabinetDevice.EnhancedConfigModeParameterName,
                    ParamName = "Enhanced Configuration Mode",
                    ParamHelp =
                        @"Indicates whether enhanced configuration options are supported in the gamePlay, commConfig, and optionConfig classes",
                    ParamCreator = () => new booleanParameter(),
                    ValueCreator = () => new booleanValue1(),
                    DefaultValue = true,
                    Value = true
                }
            };

            return BuildOptionItem(
                OptionConstants.ConfigurationDelayPeriodOptionsId,
                t_securityLevels.G2S_operator,
                1,
                1,
                "G2S_configDelayParams",
                "Configuration Delay Parameters",
                "Configuration delay parameters",
                parameters,
                includeDetails);
        }

        private optionItem BuildCashOutOnDisableOptions(bool includeDetails)
        {
            var parameters = new[]
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.CabinetDevice.CashOutOnDisableParameterName,
                    ParamName = "Cash Out On Disable Parameters",
                    ParamHelp =
                        "Method used to cash out the player upon EGM disablement",
                    ParamCreator = () => new stringParameter(),
                    ValueCreator = () => new stringValue1(),
                    Value = "G2S_forceCashOut",
                    DefaultValue = "G2S_forceCashOut"
                }
            };

            return BuildOptionItem(
                OptionConstants.CashOutOnDisableOptionsId,
                t_securityLevels.G2S_operator,
                1,
                1,
                "G2S_cashOutOnDisableParams",
                "Cash Out On Disable Parameters",
                "Cash out on disable parameters",
                parameters,
                includeDetails);
        }

        private optionItem BuildFaultsSupportedOptions(bool includeDetails)
        {
            var parameters = new[]
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.CabinetDevice.FaultsSupportedParameterName,
                    ParamName = "Cabinet Fault Support",
                    ParamHelp =
                        "Indicates whether video faults, reel tilts, etc are reported through the cabinet",
                    ParamCreator = () => new stringParameter(),
                    ValueCreator = () => new stringValue1(),
                    Value = "G2S_true",
                    DefaultValue = "G2S_true"
                }
            };

            return BuildOptionItem(
                OptionConstants.FaultsSupportedOptionsId,
                t_securityLevels.G2S_administrator,
                1,
                1,
                "G2S_faultsSupportedParams",
                "Faults Supported Option Parameters",
                "Parameters for indicating whether video faults, reel tilts, etc are reported through the cabinet",
                parameters,
                includeDetails);
        }

        private optionItem BuildTimeZonesSupportedOptions(bool includeDetails)
        {
            var parameters = new[]
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.CabinetDevice.TimeZoneSupportedParameterName,
                    ParamName = "Time Zone Support",
                    ParamHelp =
                        "Indicates whether remote time zone changes are supported",
                    ParamCreator = () => new stringParameter(),
                    ValueCreator = () => new stringValue1(),
                    DefaultValue = "G2S_unknown",
                    Value = "G2S_unknown"
                }
            };

            return BuildOptionItem(
                OptionConstants.TimeZonesSupportedOptionsId,
                t_securityLevels.G2S_administrator,
                1,
                1,
                "G2S_timeZonesSupportedParams",
                "Time Zones Supported Parameters",
                "Parameters for indicating whether remote time zone changes are supported",
                parameters,
                includeDetails);
        }

        private optionItem BuildPropertyIdentifierOptions(bool includeDetails)
        {
            var parameters = new[]
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.CabinetDevice.PropertyIdParameterName,
                    ParamName = "Property Id",
                    ParamCreator = () => new stringParameter(),
                    ValueCreator = () => new stringValue1(),
                    DefaultValue = string.Empty,
                    Value = (string)_properties.GetProperty(ApplicationConstants.PropertyId, string.Empty)
                }
            };

            return BuildOptionItem(
                OptionConstants.PropertyIdentifierOptionsId,
                t_securityLevels.G2S_attendant,
                1,
                1,
                "G2S_propertyIdParams",
                "Property Identifier Parameters",
                "Contains the property identifier associated with the EGM",
                parameters,
                includeDetails);
        }

        private optionItem BuildGameLimitsOptions(bool includeDetails)
        {
            var parameters = new[]
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.CabinetDevice.MaxEnabledThemesParameterName,
                    ParamName = "Maximum Enabled Themes",
                    ParamHelp =
                        "Maximum number of themes that can be enabled simultaneously",
                    ParamCreator = () => new integerParameter(),
                    ValueCreator = () => new integerValue1(),
                    DefaultValue = -1,
                    Value = -1
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.CabinetDevice.MaxActiveDenominationsParameterName,
                    ParamName = "Maximum Active Denoms",
                    ParamHelp =
                        "Maximum number of denoms that can be enabled simultaneously",
                    ParamCreator = () => new integerParameter(),
                    ValueCreator = () => new integerValue1(),
                    DefaultValue = -1,
                    Value = -1
                }
            };

            return BuildOptionItem(
                OptionConstants.GameLimitsOptionsId,
                t_securityLevels.G2S_administrator,
                1,
                1,
                "G2S_gameLimitsParams",
                "EGM Game Limit Parameters",
                "EGM Game Limit Parameters",
                parameters,
                includeDetails);
        }

        private optionItem BuildOccupancyMeterOptions(bool includeDetails)
        {
            var parameters = new[]
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.CabinetDevice.OccupancyTimeOutParameterName,
                    ParamName = "Occupancy Time Out",
                    ParamHelp =
                        "Amount of time while the game is idle before occupancy is suspended (in milliseconds)",
                    ParamCreator = () => new integerParameter(),
                    ValueCreator = () => new integerValue1(),
                    DefaultValue = 0,
                    Value = 0
                }
            };

            return BuildOptionItem(
                OptionConstants.OccupancyMeterOptionsId,
                t_securityLevels.G2S_administrator,
                1,
                1,
                "G2S_occupancyMeterParams",
                "Occupancy Meter Parameters",
                "Parameters for configuring the occupancy meter",
                parameters,
                includeDetails);
        }

        private optionItem BuildGtechProtocolOptions(ICabinetDevice device, bool includeDetails)
        {
            var parameters = new[]
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.CabinetDevice.MasterResetAllowedParameterName,
                    ParamName = "Remote Master Reset Allowed",
                    ParamHelp = "Remote master reset is allowed",
                    ParamCreator = () => new booleanParameter { canModRemote = true },
                    ValueCreator = () => new booleanValue1(),
                    Value = device.MasterResetAllowed,
                    DefaultValue = false
                }
            };

            return BuildOptionItem(
                OptionConstants.GtechProtocolOptionsId,
                t_securityLevels.G2S_administrator,
                1,
                1,
                "GTK_protocolParams",
                "GTECH Protocol Parameters",
                "GTECH protocol parameters for cabinet devices",
                parameters,
                includeDetails);
        }

        private optionItem BuildReelStopOptions(bool includeDetails)
        {
            var parameters = new[]
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.CabinetDevice.ReelStopParameterName,
                    ParamName = "Reel Stop",
                    ParamHelp = "Enable/disable reel stop functionality",
                    ParamCreator = () => new booleanParameter { canModLocal = true, canModRemote = true },
                    ValueCreator = () => new booleanValue1(),
                    Value = _properties.GetValue(GamingConstants.ReelStopEnabled, true),
                    DefaultValue = false
                }
            };

            return BuildOptionItem(
                OptionConstants.ReelStopOptionId,
                t_securityLevels.G2S_administrator,
                1,
                1,
                "G2S_reelStopParams",
                "Reel Stop Parameters",
                "Parameter for indicating if reel stop is enabled/disabled",
                parameters,
                includeDetails);
        }

        private optionItem BuildReelDurationOptions(bool includeDetails)
        {
            var parameters = new[]
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.CabinetDevice.ReelDurationParameterName,
                    ParamName = "Reel Duration",
                    ParamHelp = "The duration of a single game-cycle",
                    ParamCreator = () => new integerParameter { canModLocal = true, canModRemote = true },
                    ValueCreator = () => new integerValue1(),
                    Value = _properties.GetValue(
                        GamingConstants.GameRoundDurationMs,
                        GamingConstants.DefaultMinimumGameRoundDurationMs),
                    DefaultValue = GamingConstants.DefaultMinimumGameRoundDurationMs
                }
            };

            return BuildOptionItem(
                OptionConstants.ReelDurationOptionId,
                t_securityLevels.G2S_administrator,
                1,
                1,
                "G2S_reelDurationParams",
                "Reel Duration Parameters",
                "Parameter for configuring the reel spin duration",
                parameters,
                includeDetails);
        }

        private optionItem BuildBankCashoutOptions(bool includeDetails)
        {
            var parameters = new[]
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.CabinetDevice.CashoutBehaviorParameterName,
                    ParamName = "Auto Cashout Behavior for Wins Exceeding Credit Limit",
                    ParamHelp = "The behavior EGM will take for wins exceeding credit limit",
                    ParamCreator = () =>
                        new stringParameter { canModLocal = true, canModRemote = true, minLen = 4, maxLen = 50 },
                    ValueCreator = () => new stringValue1(),
                    Value = "CASHOUTFULLBANKAMOUNT",
                    DefaultValue = "CASHOUTFULLBANKAMOUNT"
                }
            };

            return BuildOptionItem(
                OptionConstants.BankCashoutOptionId,
                t_securityLevels.G2S_administrator,
                1,
                1,
                "IGT_bankCashOutParams",
                "IGT Bank Cash Out Parameters",
                "IGT Bank Cash Out Parameters",
                parameters,
                includeDetails);
        }

        private optionItem BuildCashInLimitOptions(bool includeDetails)
        {
            var parameters = new[]
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.CabinetDevice.CashInLimitParameterName,
                    ParamName = "Cash In Limit",
                    ParamHelp =
                        "The aggregate total of cashable credits accumulated from coin and currency that can exist on the EGM",
                    ParamCreator = () =>
                        new integerParameter
                        {
                            canModLocal = true, canModRemote = true, minIncl = 1000, maxIncl = 10000000000
                        },
                    ValueCreator = () => new integerValue1(),
                    Value = _properties.GetValue(
                        PropertyKey.MaxCreditsIn,
                        AccountingConstants.DefaultMaxTenderInLimit),
                    DefaultValue = AccountingConstants.DefaultMaxTenderInLimit
                }
            };

            return BuildOptionItem(
                OptionConstants.CashInLimitOptionId,
                t_securityLevels.G2S_administrator,
                1,
                1,
                "IGT_cashInLimitParams",
                "IGT Cash In Limit Parameters",
                "IGT Cash In Limit Parameters",
                parameters,
                includeDetails);
        }

        private optionItem BuildNonCashInLimitOptions(bool includeDetails)
        {
            var parameters = new[]
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.CabinetDevice.NonCashInLimitParameterName,
                    ParamName = "Non Cash In Limit",
                    ParamHelp = "Non Cash In Limit (excluding bills and coins)",
                    ParamCreator = () =>
                        new integerParameter
                        {
                            canModLocal = true, canModRemote = true, minIncl = 1000, maxIncl = 10000000000
                        },
                    ValueCreator = () => new integerValue1(),
                    Value = 100000000,
                    DefaultValue = 100000000
                }
            };

            return BuildOptionItem(
                OptionConstants.NonCashInLimitOptionId,
                t_securityLevels.G2S_administrator,
                1,
                1,
                "IGT_nonCashInLimitParams",
                "IGT Non Cash In Limit Parameters",
                "IGT Non Cash In Limit Parameters",
                parameters,
                includeDetails);
        }

        private optionItem BuildIdleTextOptions(bool includeDetails)
        {
            var parameters = new[]
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.CabinetDevice.IdleTextParameterName,
                    ParamName = "EGM Idle Text",
                    ParamHelp = "Text message to display while the EGM is in the idle state",
                    ParamCreator =
                        () => new stringParameter { canModRemote = true, canModLocal = true, maxLen = 255 },
                    ValueCreator = () => new stringValue1(),
                    DefaultValue = DefaultIdleText,
                    Value = _properties.GetValue(GamingConstants.IdleText, DefaultIdleText)
                }
            };

            return BuildOptionItem(
                OptionConstants.IdleTextOptionId,
                t_securityLevels.G2S_administrator,
                1,
                1,
                "G2S_idleTextParams",
                "Idle Text Parameters",
                "Parameter for configuring EGM Idle Text",
                parameters,
                includeDetails);
        }
    }
}