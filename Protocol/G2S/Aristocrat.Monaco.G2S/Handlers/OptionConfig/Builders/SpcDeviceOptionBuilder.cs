namespace Aristocrat.Monaco.G2S.Handlers.OptionConfig.Builders
{
    using System.Collections.Generic;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using Data.OptionConfig;

    public class SpcDeviceOptionBuilder : BaseDeviceOptionBuilder<SpcDevice>
    {
        protected override DeviceClass DeviceClass => DeviceClass.Spc;

        protected override optionGroup[] BuildGroups(SpcDevice device, OptionListCommandBuilderParameters parameters)
        {
            var group = new optionGroup
            {
                optionGroupId = "G2S_spcOptions",
                optionGroupName = "G2S Standalone Progressive Controller Options"
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
                        ParamId = G2SParametersNames.SpcDevice.ControllerTypeParameterName,
                        ParamName = "Controller Type",
                        ParamHelp = "Type of progressive controller",
                        ParamCreator = () => new stringParameter { canModRemote = true },
                        ValueCreator = () => new stringValue1(),
                        Value = device.ControllerType,
                        DefaultValue = "G2S_standaloneProg"
                    }
                };

                items.Add(
                    BuildProtocolOptions(
                        device,
                        device.RestartStatus,
                        "Standard G2S protocol parameters for the spc device",
                        additionalParameters,
                        parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.LevelTableOptionsId, parameters))
            {
                items.Add(BuildLevelTableOptions(device, parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.GameConfigTableOptionsId, parameters))
            {
                items.Add(BuildGameConfigTableOptions(device, parameters.IncludeDetails));
            }

            group.optionItem = items.ToArray();

            return new[] { group };
        }

        private optionItem BuildLevelTableOptions(SpcDevice device, bool includeDetails)
        {
            var parameters = new[]
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.SpcDevice.LevelIdParameterName,
                    ParamName = "Standalone progressive level ID",
                    ParamHelp = "Represents the standalone progressive level to which the settings will be applied",
                    ParamCreator =
                        () => new integerParameter
                        {
                            canModRemote = true, canModLocal = true, minIncl = 1, maxIncl = 32
                        },
                    ValueCreator = () => new integerValue1(),
                    Value = device.LevelId,
                    DefaultValue = 1
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.SpcDevice.ResetAmountParameterName,
                    ParamName = "Standalone progressive reset amount",
                    ParamHelp =
                        "Represents the reset standalone progressive amount after a progressive award has been hit",
                    ParamCreator = () => new integerParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new integerValue1(),
                    Value = device.ResetAmount,
                    DefaultValue = 100000000
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.SpcDevice.MaxLevelAmountParameterName,
                    ParamName = "Standalone progressive max amount",
                    ParamHelp = "Represents the maximum amount of the standalone progressive",
                    ParamCreator = () => new integerParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new integerValue1(),
                    Value = device.MaxLevelAmount,
                    DefaultValue = 1000000000
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.SpcDevice.ContribPercentParameterName,
                    ParamName = "Standalone progressive contribution percentage",
                    ParamHelp = @"Represents the standalone contribution percentage. This percentage \
                    of each qualifying wager is added to the progressive level value. Precision is \
                    ten-thousandths of a percent (0.0001%)",
                    ParamCreator = () => new integerParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new integerValue1(),
                    Value = device.ContribPercent,
                    DefaultValue = 15000
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.SpcDevice.RoundingEnabledParameterName,
                    ParamName = "Standalone progressive rounding behavior",
                    ParamHelp = @"When configured for rounding, the standalone progressive \
                    controller will round the progressive award up to the nearest player \
                    denomination",
                    ParamCreator = () => new booleanParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new booleanValue1(),
                    Value = device.RoundingEnabled,
                    DefaultValue = true
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.SpcDevice.MystMinParameterName,
                    ParamName = "Standalone progressive mystery minimum",
                    ParamHelp = "The low-end of the mystery number range",
                    ParamCreator = () => new integerParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new integerValue1(),
                    Value = device.MysteryMinimum,
                    DefaultValue = 500000000
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.SpcDevice.MystMaxParameterName,
                    ParamName = "Standalone progressive mystery maximum",
                    ParamHelp = "The high-end of the mystery number range",
                    ParamCreator = () => new integerParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new integerValue1(),
                    Value = device.MysteryMaximum,
                    DefaultValue = 1000000000
                }
            };

            return BuildOptionItem(
                OptionConstants.LevelTableOptionsId,
                t_securityLevels.G2S_operator,
                0,
                32,
                G2SParametersNames.SpcDevice.LevelDataParameterName,
                "Standalone progressive controller level data",
                "Information on a standalone progressive level of this device",
                parameters,
                includeDetails);
        }

        private optionItem BuildGameConfigTableOptions(SpcDevice device, bool includeDetails)
        {
            var parameters = new[]
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.SpcDevice.LevelIdParameterName,
                    ParamName = "Standalone progressive level ID",
                    ParamHelp = "Represents the standalone progressive level to which the settings will be applied",
                    ParamCreator = () => new integerParameter { canModRemote = true, canModLocal = true, minIncl = 1, maxIncl = 32},
                    ValueCreator = () => new integerValue1(),
                    Value = device.LevelId,
                    DefaultValue = 2
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.SpcDevice.GamePlayIdParameterName,
                    ParamName = "Game Play Device ID",
                    ParamHelp = "Game play device ID for this level",
                    ParamCreator = () => new integerParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new integerValue1(),
                    Value = device.GamePlayId,
                    DefaultValue = 0
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.SpcDevice.WinLevelIndexParameterName,
                    ParamName = "Win Level Index",
                    ParamHelp = "Paytable win-level index required to win this level",
                    ParamCreator = () => new integerParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new integerValue1(),
                    Value = device.WinLevelIndex,
                    DefaultValue = 0
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.SpcDevice.PaytableIdParameterName,
                    ParamName = "Paytable ID",
                    ParamHelp = "Paytable ID associated with the game play device",
                    ParamCreator = () => new stringParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new stringValue1(),
                    Value = device.PaytableId,
                    DefaultValue = string.Empty
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.SpcDevice.ThemeIdParameterName,
                    ParamName = "Theme ID",
                    ParamHelp = "Theme ID associated with the game play device",
                    ParamCreator = () => new stringParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new stringValue1(),
                    Value = device.ThemeId,
                    DefaultValue = string.Empty
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.SpcDevice.DenomIdParameterName,
                    ParamName = "Denomination ID",
                    ParamHelp = "Denomination ID associated with this win-level index",
                    ParamCreator = () => new integerParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new integerValue1(),
                    Value = device.DenomId,
                    DefaultValue = 100000
                }
            };

            return BuildOptionItem(
                OptionConstants.GameConfigTableOptionsId,
                t_securityLevels.G2S_operator,
                0,
                999,
                G2SParametersNames.SpcDevice.GameConfigDataParameterName,
                "Standalone progressive controller game associations",
                "Game associated with this standalone progressive level",
                parameters,
                includeDetails);
        }
    }
}
