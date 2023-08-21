namespace Aristocrat.Monaco.G2S.Handlers.OptionConfig.Builders
{
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using Data.OptionConfig;
    using Gaming.Contracts;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ChooserDeviceOptionBuilder : BaseDeviceOptionBuilder<ChooserDevice>
    {
        private readonly IGameProvider _gameProvider;
        private readonly IGameOrderSettings _gameOrderSettings;

        public ChooserDeviceOptionBuilder(IGameProvider gameProvider, IGameOrderSettings gameOrderService)
        {
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _gameOrderSettings = gameOrderService ?? throw new ArgumentNullException(nameof(gameOrderService));
        }

        protected override DeviceClass DeviceClass => DeviceClass.Chooser;

        protected override optionGroup[] BuildGroups(
            ChooserDevice device,
            OptionListCommandBuilderParameters parameters)
        {
            var group = new optionGroup
            {
                optionGroupId = "G2S_chooserOptions",
                optionGroupName = "G2S Chooser Options"
            };

            var items = new List<optionItem>();

            if (ShouldIncludeParam(OptionConstants.ProtocolOptionsId, parameters))
            {
                items.Add(BuildProtocolParameters(device, parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.GameComboPositionDataTable, parameters))
            {
                items.Add(BuildGameComboPositionDataTable(parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.GameComboTagDataTable, parameters))
            {
                items.Add(BuildGameComboTagDataTable(parameters.IncludeDetails));
            }

            group.optionItem = items.ToArray();

            return new[] { group };
        }

        private optionItem BuildProtocolParameters(IChooserDevice device, bool includeDetails)
        {
            var parameters = new List<ParameterDescription>
            {
                ConfigIdParameter(device.ConfigurationId),
                UseDefaultConfigParameter(device.UseDefaultConfig),
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.ConfigDateTimeParameterName,
                    ParamName = "Configuration Date/Time",
                    ParamHelp = "Date/time device configuration was last changed.",
                    ParamCreator = () => new stringParameter(),
                    ValueCreator = () => new stringValue1(),
                    Value = device.ConfigDateTime == DateTime.MinValue
                        ? string.Empty
                        : device.ConfigDateTime.ToUniversalTime().ToString("o"),
                    DefaultValue = string.Empty
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.ConfigCompleteParameterName,
                    ParamName = "Configuration Complete",
                    ParamHelp = "Indicates whether the device configuration is complete.",
                    ParamCreator = () => new booleanParameter(),
                    ValueCreator = () => new booleanValue1(),
                    Value = device.ConfigComplete,
                    DefaultValue = true
                }
            };

            return BuildOptionItem(
                OptionConstants.ProtocolOptionsId,
                t_securityLevels.G2S_administrator,
                1,
                1,
                "G2S_protocolParams",
                "G2S Protocol Parameters",
                "Standard G2S protocol parameters for chooser devices",
                parameters,
                includeDetails);
        }

        private optionItem BuildGameComboPositionDataTable(bool includeDetails)
        {
            var parameters = new List<ParameterDescription>
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.ChooserDevice.GamePlayIdParameterName,
                    ParamName = "Game Play Device ID",
                    ParamHelp = "Game play device ID",
                    ParamCreator = () => new integerParameter()
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.ChooserDevice.ThemeIdParameterName,
                    ParamName = "Theme ID",
                    ParamHelp = "Theme identifier",
                    ParamCreator = () => new stringParameter()
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.ChooserDevice.PaytableIdParameterName,
                    ParamName = "Paytable ID",
                    ParamHelp = "Paytable identifier",
                    ParamCreator = () => new stringParameter()
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.ChooserDevice.DenomIdParameterName,
                    ParamName = "Denomination ID",
                    ParamHelp = "Denomination identifier",
                    ParamCreator = () => new integerParameter()
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.ChooserDevice.PositionParameterName,
                    ParamName = "Game Position Priority",
                    ParamHelp = "Game position priority applied to the game combo",
                    ParamCreator = () => new integerParameter{canModLocal = true, canModRemote = true}
                }
            };

            var currentValues = _gameProvider.GetGames().Select(g => CreateGamePositionComplexValue(g, _gameOrderSettings.GetIconPositionPriority(g.ThemeId)));

            var defaultValues = _gameProvider.GetGames().Select(g => CreateGamePositionComplexValue(g, 1)).ToList();

            return BuildOptionItemForTable(
                OptionConstants.GameComboPositionDataTable,
                t_securityLevels.G2S_operator,
                0,
                defaultValues.Count,
                G2SParametersNames.ChooserDevice.GameComboPositionParameterName,
                "Game Combo Position Priority Data Table",
                "Position Priority of Game Combo",
                parameters,
                currentValues,
                defaultValues,
                includeDetails);
        }

        private complexValue CreateGamePositionComplexValue(IGameDetail g, long position)
        {
            return new complexValue
            {
                paramId = G2SParametersNames.ChooserDevice.GameComboPositionParameterName,
                Items = new object[]
                {
                    new integerValue1
                    {
                        paramId = G2SParametersNames.ChooserDevice.GamePlayIdParameterName,
                        Value = g.Id
                    },
                    new stringValue1
                    {
                        paramId = G2SParametersNames.ChooserDevice.ThemeIdParameterName,
                        Value = g.ThemeId
                    },
                    new stringValue1
                    {
                        paramId = G2SParametersNames.ChooserDevice.PaytableIdParameterName,
                        Value = g.PaytableId
                    },
                    new integerValue1
                    {
                        paramId = G2SParametersNames.ChooserDevice.DenomIdParameterName,
                        Value = g.ActiveDenominations.FirstOrDefault() // At this time there is only one denomination per game
                    },
                    new integerValue1
                    {
                        paramId = G2SParametersNames.ChooserDevice.PositionParameterName,
                        Value = position
                    }
                }
            };
        }

        private optionItem BuildGameComboTagDataTable(bool includeDetails)
        {
            var parameters = new List<ParameterDescription>
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.ChooserDevice.GamePlayIdParameterName,
                    ParamName = "Game Play Device ID",
                    ParamHelp = "Game play device ID",
                    ParamCreator = () => new integerParameter{ canModLocal = true, canModRemote = true }
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.ChooserDevice.ThemeIdParameterName,
                    ParamName = "Theme ID",
                    ParamHelp = "Theme identifier",
                    ParamCreator = () => new stringParameter{ canModLocal = true, canModRemote = true }
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.ChooserDevice.PaytableIdParameterName,
                    ParamName = "Paytable ID",
                    ParamHelp = "Paytable identifier",
                    ParamCreator = () => new stringParameter{ canModLocal = true, canModRemote = true }
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.ChooserDevice.DenomIdParameterName,
                    ParamName = "Denomination ID",
                    ParamHelp = "Denomination identifier",
                    ParamCreator = () => new integerParameter{ canModLocal = true, canModRemote = true }
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.ChooserDevice.GameTagParameterName,
                    ParamName = "Game Tag",
                    ParamHelp = "Game tag applied to the game combo",
                    ParamCreator = () => new stringParameter{canModLocal = true, canModRemote = true}
                }
            };

            var currentValues = new List<complexValue>();
            foreach (var game in _gameProvider.GetGames())
            {
                if (!game.GameTags.Any())
                {
                    // Add a game combo tag with a default empty string value for all combos that have no tags so they can be added easily
                    currentValues.Add(CreateGameTagComplexValue(game, string.Empty));
                }
                else
                {
                    currentValues.AddRange(game.GameTags.Select(tag => CreateGameTagComplexValue(game, tag)));
                }
            }

            // Default game tag for each combo is empty string
            var defaultValues = _gameProvider.GetGames().Select(g => CreateGameTagComplexValue(g, string.Empty));

            return BuildOptionItemForTable(
                OptionConstants.GameComboTagDataTable,
                t_securityLevels.G2S_operator,
                0,
                99, // there can be multiple entries per game combo to add more tags
                G2SParametersNames.ChooserDevice.GameTagDataParameterName,
                "Game Combo Tag Data Table",
                "Game Combo tags",
                parameters,
                currentValues,
                defaultValues,
                includeDetails);
        }

        private complexValue CreateGameTagComplexValue(IGameDetail g, string tag)
        {
            return new complexValue
            {
                paramId = G2SParametersNames.ChooserDevice.GameTagDataParameterName,
                Items = new object[]
                {
                    new integerValue1
                    {
                        paramId = G2SParametersNames.ChooserDevice.GamePlayIdParameterName,
                        Value = g.Id
                    },
                    new stringValue1
                    {
                        paramId = G2SParametersNames.ChooserDevice.ThemeIdParameterName,
                        Value = g.ThemeId
                    },
                    new stringValue1
                    {
                        paramId = G2SParametersNames.ChooserDevice.PaytableIdParameterName,
                        Value = g.PaytableId
                    },
                    new integerValue1
                    {
                        paramId = G2SParametersNames.ChooserDevice.DenomIdParameterName,
                        Value = g.ActiveDenominations.FirstOrDefault()
                    },
                    new stringValue1
                    {
                        paramId = G2SParametersNames.ChooserDevice.GameTagParameterName,
                        Value = tag.ToG2SGameTagString()
                    }
                }
            };
        }
    }
}
