namespace Aristocrat.Monaco.G2S.Handlers.OptionConfig.Builders
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts.Localization;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using Data.OptionConfig;
    using Gaming.Contracts.Bonus;
    using Localization.Properties;

    public class BonusDeviceOptionBuilder : BaseDeviceOptionBuilder<BonusDevice>
    {
        private readonly IBonusHandler _bonusHandler;

        public BonusDeviceOptionBuilder(IBonusHandler bonusHandler)
        {
            _bonusHandler = bonusHandler ?? throw new ArgumentNullException(nameof(bonusHandler));
        }

        protected override DeviceClass DeviceClass => DeviceClass.Bonus;

        protected override optionGroup[] BuildGroups(BonusDevice device, OptionListCommandBuilderParameters parameters)
        {
            var items = new List<optionItem>();

            var group = new optionGroup { optionGroupId = "G2S_bonusOptions", optionGroupName = "G2S Bonus Options" };

            if (ShouldIncludeParam(OptionConstants.ProtocolOptionsId, parameters))
            {
                var additionalParameters = new[]
                {
                    new ParameterDescription
                    {
                        ParamId = G2SParametersNames.TimeToLiveParameterName,
                        ParamName = "Time To Live",
                        ParamHelp = "Time to live value for requests (in milliseconds)",
                        ParamCreator = () => new integerParameter { canModRemote = true },
                        ValueCreator = () => new integerValue1(),
                        Value = device.TimeToLive,
                        DefaultValue = (int)Constants.DefaultTimeout.TotalMilliseconds
                    },
                    new ParameterDescription
                    {
                        ParamId = G2SParametersNames.BonusDevice.NoMessageTimerParameterName,
                        ParamName = "No Message Timer",
                        ParamHelp = "Max time to wait for a message from owner host (in milliseconds)",
                        ParamCreator = () => new integerParameter { canModRemote = true },
                        ValueCreator = () => new integerValue1(),
                        Value = (int)device.NoResponseTimer.TotalMilliseconds,
                        DefaultValue = (int)Constants.DefaultTimeout.TotalMilliseconds
                    },
                    new ParameterDescription
                    {
                        ParamId = G2SParametersNames.BonusDevice.NoHostTextParameterName,
                        ParamName = "No Host Text",
                        ParamHelp = "Text to display if bonus host communications are lost",
                        ParamCreator = () => new stringParameter { canModRemote = true, maxLen = 256 },
                        ValueCreator = () => new stringValue1(),
                        Value = device.NoHostText ?? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NoBonusHost),
                        DefaultValue = string.Empty
                    },
                    new ParameterDescription
                    {
                        ParamId = G2SParametersNames.BonusDevice.IdReaderIdParameterName,
                        ParamName = "ID Reader to Use",
                        ParamHelp = "ID reader to use for bonuses (0 = None)",
                        ParamCreator = () => new integerParameter { canModRemote = true },
                        ValueCreator = () => new integerValue1(),
                        Value = device.IdReaderId,
                        DefaultValue = 0
                    },
                    new ParameterDescription
                    {
                        ParamId = G2SParametersNames.BonusDevice.MaxPendingBonusParameterName,
                        ParamName = "Maximum Pending Bonuses",
                        ParamHelp = "Maximum number of pending bonuses EGM supports",
                        ParamCreator = () => new integerParameter { canModRemote = true },
                        ValueCreator = () => new integerValue1(),
                        Value = _bonusHandler.MaxPending,
                        DefaultValue = 8
                    }
                };

                items.Add(
                    BuildProtocolOptions(
                        device,
                        device.RestartStatus,
                        "Standard G2S protocol parameters for bonus devices",
                        additionalParameters,
                        parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.ProtocolAdditionalOptionsId, parameters))
            {
                items.Add(BuildProtocolAdditionalOptions(device, parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.BonusUsePlayerIdReaderOptionsId, parameters))
            {
                items.Add(BuildIdReaderOptions(device, parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.BonusOptionsOptionId, parameters))
            {
                items.Add(BuildBonusOptions(device, parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.WagerMatchBonusConfig, parameters))
            {
                items.Add(BuildWagerMatchOptions(device, parameters.IncludeDetails));
            }

            group.optionItem = items.ToArray();

            return new[] { group };
        }

        private optionItem BuildIdReaderOptions(BonusDevice device, bool includeDetails)
        {
            var parameters = new[]
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.BonusDevice.UsePlayerIdReaderParameterName,
                    ParamName = "Use Player ID Reader Parameters",
                    ParamHelp =
                        "Indicates whether the ID reader associated with the currently active player session should be used.",
                    ParamCreator = () => new booleanParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new booleanValue1(),
                    Value = device.UsePlayerIdReader,
                    DefaultValue = false
                }
            };

            return BuildOptionItem(
                OptionConstants.BonusUsePlayerIdReaderOptionsId,
                t_securityLevels.G2S_operator,
                1,
                1,
                "G2S_usePlayerIdReaderParams",
                "Use Player ID Reader",
                "Parameters associated with Use-Player-ID-Reader option.",
                parameters,
                includeDetails);
        }

        private optionItem BuildBonusOptions(BonusDevice device, bool includeDetails)
        {
            var parameters = new[]
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.BonusDevice.BonusEligibilityTimer,
                    ParamName = "Bonus Eligibility Timer",
                    ParamHelp =
                        "The time from the last game started that an eligibility-tested setBonusAward command will be paid.",
                    ParamCreator = () => new integerParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new integerValue1(),
                    Value = (int)device.EligibilityTimer.TotalMilliseconds,
                    DefaultValue = (int)G2SParametersNames.BonusDevice.EligibilityTimerDefault.TotalMilliseconds
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.BonusDevice.MaximumBonusLimit,
                    ParamName = "Maximum bonus limit",
                    ParamHelp = "The maximum bonus award amount that the EGM will pay.",
                    ParamCreator = () => new integerParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new integerValue1(),
                    Value = device.DisplayLimit,
                    DefaultValue = G2SParametersNames.BonusDevice.LimitDefault
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.BonusDevice.MaximumBonusLimitText,
                    ParamName = "Maximum bonus message",
                    ParamHelp = "Alternate text to display when a bonus award exceeds the maximum.",
                    ParamCreator = () => new stringParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new stringValue1(),
                    Value = device.DisplayLimitText ?? Localizer.For(CultureFor.Player).GetString(ResourceKeys.BonusLimitExceeded),
                    DefaultValue = Localizer.For(CultureFor.Player).GetString(ResourceKeys.BonusLimitExceeded)
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.BonusDevice.DisplayLimitDuration,
                    ParamName = "Maximum bonus text duration",
                    ParamHelp = "The length of time to display the Maximum bonus message.",
                    ParamCreator = () => new integerParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new integerValue1(),
                    Value = (int)device.DisplayLimitDuration.TotalMilliseconds,
                    DefaultValue = (int)G2SParametersNames.BonusDevice.DisplayLimitDurationDefault.TotalMilliseconds
                }
            };

            return BuildOptionItem(
                OptionConstants.BonusOptionsOptionId,
                t_securityLevels.G2S_administrator,
                1,
                1,
                "IGT_bonusLimits",
                "IGT Bonus Limits",
                "Bonus Limits",
                parameters,
                includeDetails);
        }

        private optionItem BuildWagerMatchOptions(BonusDevice device, bool includeDetails)
        {
            var parameters = new[]
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.BonusDevice.WagerMatchCardRequired,
                    ParamName = "Card Required For Wager Match",
                    ParamHelp = "Fail any Wager Match payments when there is no active player session.",
                    ParamCreator = () => new booleanParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new booleanValue1(),
                    Value = device.WagerMatchCardRequired,
                    DefaultValue = false
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.BonusDevice.WagerMatchLimit,
                    ParamName = "Wager Match limit",
                    ParamHelp = "Maximum single Wager Match authorization.",
                    ParamCreator = () => new integerParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new integerValue1(),
                    Value = device.WagerMatchLimit,
                    DefaultValue = G2SParametersNames.BonusDevice.LimitDefault
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.BonusDevice.WagerMatchLimitText,
                    ParamName = "Wager Match limit message",
                    ParamHelp = "Message to display when a single Wager Match authorization exceeds the limit.",
                    ParamCreator = () => new stringParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new stringValue1(),
                    Value = device.WagerMatchLimitText ?? Localizer.For(CultureFor.Player).GetString(ResourceKeys.BonusLimitExceeded),
                    DefaultValue = Localizer.For(CultureFor.Player).GetString(ResourceKeys.BonusLimitExceeded)
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.BonusDevice.WagerMatchLimitDuration,
                    ParamName = "Wager Match limit message duration",
                    ParamHelp = "Length of time to display the Wager Match limit text.",
                    ParamCreator = () => new integerParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new integerValue1(),
                    Value = (int)device.WagerMatchLimitDuration.TotalMilliseconds,
                    DefaultValue = (int)G2SParametersNames.BonusDevice.WagerMatchLimitDurationDefault.TotalMilliseconds
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.BonusDevice.WagerMatchExitText,
                    ParamName = "Wager Match Exit message",
                    ParamHelp = "Message to display when a Wager Match authorization is closed.",
                    ParamCreator = () => new stringParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new stringValue1(),
                    Value = device.WagerMatchExitText ?? Localizer.For(CultureFor.Player).GetString(ResourceKeys.WagerMatchExitText),
                    DefaultValue = Localizer.For(CultureFor.Player).GetString(ResourceKeys.WagerMatchExitText)
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.BonusDevice.WagerMatchExitDuration,
                    ParamName = "Wager Match Exit message duration",
                    ParamHelp = "Length of time to display message when a Wager Match authorization is closed.",
                    ParamCreator = () => new integerParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new integerValue1(),
                    Value = (int)device.WagerMatchExitDuration.TotalMilliseconds,
                    DefaultValue = (int)G2SParametersNames.BonusDevice.WagerMatchExitDurationDefault.TotalMilliseconds
                }
            };

            return BuildOptionItem(
                OptionConstants.WagerMatchBonusConfig,
                t_securityLevels.G2S_administrator,
                1,
                1,
                "IGT_bonusWM",
                "IGT Wager Match Options",
                "Wager Match parameters",
                parameters,
                includeDetails);

        }
    }
}