namespace Aristocrat.Monaco.G2S.Handlers.OptionConfig.Builders
{
    using System.Collections.Generic;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using Data.OptionConfig;

    /// <inheritdoc />
    public class InformedPlayerDeviceOptionBuilder : BaseDeviceOptionBuilder<InformedPlayerDevice>
    {
        /// <inheritdoc />
        protected override DeviceClass DeviceClass => DeviceClass.InformedPlayer;

        /// <inheritdoc />
        protected override optionGroup[] BuildGroups(
            InformedPlayerDevice device,
            OptionListCommandBuilderParameters parameters)
        {
            var group = new optionGroup
            {
                optionGroupId = OptionConstants.InformedPlayerOptionsId,
                optionGroupName = "G2S Informed Player Options"
            };

            var items = new List<optionItem>();

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
                        ParamId = G2SParametersNames.InformedPlayerDevice.NoMessageTimerParameterName,
                        ParamName = "No Message Timer",
                        ParamHelp =
                            "Maximum time period without communications before Informed Player functionality is disabled",
                        ParamCreator = () => new integerParameter { canModRemote = true },
                        ValueCreator = () => new integerValue1(),
                        Value = device.NoMessageTimer,
                        DefaultValue = InformedPlayerDevice.DefaultNoMessageTimer
                    },
                    new ParameterDescription
                    {
                        ParamId = G2SParametersNames.InformedPlayerDevice.NoHostTextParameterName,
                        ParamName = "No Host Text",
                        ParamHelp =
                            "Message to display while Informed Player functionality is disabled due to a loss of communications",
                        ParamCreator = () => new stringParameter { canModRemote = true },
                        ValueCreator = () => new stringValue1(),
                        Value = device.NoHostText,
                        DefaultValue = InformedPlayerDevice.DefaultNoHostText
                    }
                };

                items.Add(
                    BuildProtocolOptions(
                        device,
                        device.RestartStatus,
                        "Standard G2S protocol parameters for informed player devices",
                        additionalParameters,
                        parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.InformedPlayerOptionsId, parameters))
            {
                items.Add(BuildInformedPlayerOptions(device, parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.ProtocolAdditionalOptionsId, parameters))
            {
                items.Add(BuildProtocolAdditionalOptions(device, parameters.IncludeDetails));
            }

            group.optionItem = items.ToArray();

            return new[] { group };
        }

        private optionItem BuildInformedPlayerOptions(IInformedPlayerDevice device, bool includeDetails)
        {
            var parameters = new[]
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.InformedPlayerDevice.UncardedMoneyInParameterName,
                    ParamName = "Uncarded Money In",
                    ParamHelp = "Enable money-in for uncarded sessions",
                    ParamCreator = () => new booleanParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new booleanValue1(),
                    Value = device.UnCardedMoneyIn,
                    DefaultValue = InformedPlayerDevice.DefaultUncardedMoneyIn
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.InformedPlayerDevice.UncardedGamePlayParameterName,
                    ParamName = "Uncarded Game Play",
                    ParamHelp = "Enable game-play for uncarded sessions",
                    ParamCreator = () => new booleanParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new booleanValue1(),
                    Value = device.UnCardedGamePlay,
                    DefaultValue = InformedPlayerDevice.DefaultUncardedGamePlay
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.InformedPlayerDevice.SessionStartMoneyInParameterName,
                    ParamName = "Enable Money In on Session Start",
                    ParamHelp = "Enable money-in upon player session start",
                    ParamCreator = () => new booleanParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new booleanValue1(),
                    Value = device.SessionStartMoneyIn,
                    DefaultValue = InformedPlayerDevice.DefaultSessionStartMoneyIn
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.InformedPlayerDevice.SessionStartGamePlayParameterName,
                    ParamName = "Enable GamePlay on Session Start",
                    ParamHelp = "Enable game-play upon player session start",
                    ParamCreator = () => new booleanParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new booleanValue1(),
                    Value = device.SessionStartGamePlay,
                    DefaultValue = InformedPlayerDevice.DefaultSessionStartGamePlay
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.InformedPlayerDevice.SessionStartCashOutParameterName,
                    ParamName = "Cash Out on Session Start",
                    ParamHelp = "On player session start, force an EGM cash-out if a current balance exists",
                    ParamCreator = () => new booleanParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new booleanValue1(),
                    Value = device.SessionStartCashOut,
                    DefaultValue = InformedPlayerDevice.DefaultSessionStartCashOut
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.InformedPlayerDevice.SessionEndCashOutParameterName,
                    ParamName = "Cash Out on Session End",
                    ParamHelp = "On player session end, force an EGM cash-out if a current balance exists",
                    ParamCreator = () => new booleanParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new booleanValue1(),
                    Value = device.SessionEndCashOut,
                    DefaultValue = InformedPlayerDevice.DefaultSessionEndCashOut
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.InformedPlayerDevice.SessionStartPinEntryParameterName,
                    ParamName = "PIN Entry on Session Start",
                    ParamHelp =
                        "On player session start, indicates whether PIN entry may be required for some or all players",
                    ParamCreator = () => new booleanParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new booleanValue1(),
                    Value = device.SessionStartPinEntry,
                    DefaultValue = InformedPlayerDevice.DefaultSessionStartPinEntry
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.InformedPlayerDevice.SessionStartLimitParameterName,
                    ParamName = "Session Update Frequency on Session Start",
                    ParamHelp = "On player session start, frequency at which session update events are generated",
                    ParamCreator = () => new integerParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new integerValue1(),
                    Value = device.SessionStartLimit,
                    DefaultValue = InformedPlayerDevice.DefaultSessionStartLimit
                }
            };

            return BuildOptionItem(
                OptionConstants.InformedPlayerOptionsId,
                t_securityLevels.G2S_operator,
                1,
                1,
                "G2S_informedPlayerParams",
                "Informed Player Parameters",
                "Configuration settings for this Informed Player device",
                parameters,
                includeDetails);
        }
    }
}
