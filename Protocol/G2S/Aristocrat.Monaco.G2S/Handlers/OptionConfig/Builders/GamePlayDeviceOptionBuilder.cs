namespace Aristocrat.Monaco.G2S.Handlers.OptionConfig.Builders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using Data.OptionConfig;
    using Gaming.Contracts;
    using Gaming.Contracts.Progressives;

    /// <inheritdoc />
    public class GamePlayDeviceOptionBuilder : BaseDeviceOptionBuilder<GamePlayDevice>
    {
        private readonly IGameProvider _games;
        private readonly IProtocolLinkedProgressiveAdapter _progressives;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GamePlayDeviceOptionBuilder" /> class.
        /// </summary>
        public GamePlayDeviceOptionBuilder(IGameProvider games, IProtocolLinkedProgressiveAdapter progressiveConfiguration)
        {
            _games = games ?? throw new ArgumentNullException(nameof(games));
            _progressives = progressiveConfiguration ?? throw new ArgumentNullException(nameof(progressiveConfiguration));
        }

        /// <inheritdoc />
        protected override DeviceClass DeviceClass => DeviceClass.GamePlay;

        /// <inheritdoc />
        protected override optionGroup[] BuildGroups(
            GamePlayDevice device,
            OptionListCommandBuilderParameters parameters)
        {
            var group = new optionGroup
            {
                optionGroupId = "G2S_gamePlayOptions",
                optionGroupName = "G2S Game Play Options"
            };

            var items = new List<optionItem>();

            if (ShouldIncludeParam(OptionConstants.ProtocolOptionsId, parameters))
            {
                items.Add(
                    BuildProtocolOptions(
                        device,
                        device.RestartStatus,
                        "Standard G2S protocol parameters for the game play device",
                        parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.GamePlayOptionsId, parameters))
            {
                items.Add(BuildGamePlayOptions(device, parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.GameTypeOptionsId, parameters))
            {
                items.Add(BuildGameTypeOptions(device, parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.GameAccessOptionsId, parameters))
            {
                items.Add(BuildGameAccessOptions(device, parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.GameAccessibleOptionsId, parameters))
            {
                items.Add(BuildGameAccessibleOptions(device, parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.GameDenomListOptionsId, parameters))
            {
                items.Add(BuildGameDenomListOptions(device, parameters.IncludeDetails));
            }

            group.optionItem = items.ToArray();

            return new[] { group };
        }

        private optionItem BuildGamePlayOptions(GamePlayDevice device, bool includeDetails)
        {
            var game = _games.GetGame(device.Id);

            var parameters = new[]
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.GamePlayDevice.ThemeIdParameterName,
                    ParamName = "Theme ID",
                    ParamHelp = "Theme for this device",
                    ParamCreator = () => new stringParameter(),
                    ValueCreator = () => new stringValue1(),
                    Value = game?.ThemeId,
                    DefaultValue = game?.ThemeId
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.GamePlayDevice.PaytableIdParameterName,
                    ParamName = "Paytable ID",
                    ParamHelp = "Paytable for this device",
                    ParamCreator = () => new stringParameter(),
                    ValueCreator = () => new stringValue1(),
                    Value = game?.PaytableId,
                    DefaultValue = game?.PaytableId
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.GamePlayDevice.MaxWagerCreditsParameterName,
                    ParamName = "Max Wager (credits)",
                    ParamHelp = "Maximum wager (in credits)",
                    ParamCreator = () => new integerParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new integerValue1(),
                    Value = game?.MaximumWagerCredits,
                    DefaultValue = game?.MaximumWagerCredits ?? 1
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.GamePlayDevice.ProgAllowedParameterName,
                    ParamName = "Progressive Allowed",
                    ParamHelp = "Progressive allowed for this device",
                    ParamCreator = () => new booleanParameter(),
                    ValueCreator = () => new booleanValue1(),
                    Value = _progressives.ViewProgressiveLevels().Any(p => p.GameId == game?.Id),
                    DefaultValue = false
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.GamePlayDevice.SecondaryAllowedParameterName,
                    ParamName = "Secondary Allowed",
                    ParamHelp = "Secondary game allowed for this device",
                    ParamCreator = () => new booleanParameter(),
                    ValueCreator = () => new booleanValue1(),
                    Value = game?.Denominations.Any(d => d.SecondaryAllowed),
                    DefaultValue = false
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.GamePlayDevice.CentralAllowedParameterName,
                    ParamName = "Central Determination",
                    ParamHelp = "Outcomes determined by central determination host",
                    ParamCreator = () => new booleanParameter(),
                    ValueCreator = () => new booleanValue1(),
                    Value = game?.CentralAllowed,
                    DefaultValue = false
                }
            };

            return BuildOptionItem(
                OptionConstants.GamePlayOptionsId,
                t_securityLevels.G2S_operator,
                1,
                1,
                "G2S_gamePlayParams",
                "Game play Parameters",
                "Configuration settings for this game play device",
                parameters,
                includeDetails);
        }

        private optionItem BuildGameTypeOptions(GamePlayDevice device, bool includeDetails)
        {
            var parameters = new[]
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.GamePlayDevice.StandardPlayParameterName,
                    ParamName = "Standard Play",
                    ParamHelp = "Indicates whether the game can be used for standard play.",
                    ParamCreator = () => new booleanParameter(),
                    ValueCreator = () => new booleanValue1(),
                    Value = true,
                    DefaultValue = true
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.GamePlayDevice.TournamentPlayParameterName,
                    ParamName = "Tournament Play",
                    ParamHelp = "Indicates whether the game can be used for tournament play.",
                    ParamCreator = () => new booleanParameter(),
                    ValueCreator = () => new booleanValue1(),
                    Value = false,
                    DefaultValue = false
                }
            };

            return BuildOptionItem(
                OptionConstants.GameTypeOptionsId,
                t_securityLevels.G2S_administrator,
                1,
                1,
                "G2S_gameTypeParams",
                "Game Type Parameters",
                "Parameters used to identify tournament and standard games.",
                parameters,
                includeDetails);
        }

        private optionItem BuildGameAccessOptions(GamePlayDevice device, bool includeDetails)
        {
            var parameter = new ParameterDescription
            {
                ParamId = OptionConstants.GameAccessOptionsId,
                ParamName = "Set Game Access Via Option Configuration",
                ParamHelp = "Indicates whether accessible games and denominations can be set via option configuration.",
                ParamCreator = () => new booleanParameter(),
                ValueCreator = () => new booleanValue1(),
                Value = device.SetViaAccessConfig,
                DefaultValue = false
            };

            return BuildOptionItemOfNonComplexType(
                OptionConstants.GameAccessOptionsId,
                t_securityLevels.G2S_administrator,
                1,
                1,
                "G2S_setAccessViaConfig",
                "Set Game Access Via Option Configuration",
                "Indicates whether accessible games and denominations can be set via option configuration.",
                parameter,
                includeDetails);
        }

        private optionItem BuildGameAccessibleOptions(GamePlayDevice device, bool includeDetails)
        {
            var parameter = new ParameterDescription
            {
                ParamId = OptionConstants.GameAccessibleOptionsId,
                ParamName = "Game Accessible to Player",
                ParamHelp = "Indicates whether the game should be accessible to the player.",
                ParamCreator = () => new booleanParameter(),
                ValueCreator = () => new booleanValue1(),
                Value = true,
                DefaultValue = true
            };

            return BuildOptionItemOfNonComplexType(
                OptionConstants.GameAccessibleOptionsId,
                t_securityLevels.G2S_operator,
                1,
                1,
                "G2S_accessibleGame",
                "Game Accessible to Player",
                "Indicates whether the game should be accessible to the player.",
                parameter,
                includeDetails);
        }

        private optionItem BuildGameDenomListOptions(GamePlayDevice device, bool includeDetails)
        {
            var parameters = new[]
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.GamePlayDevice.DenomIdParameterName,
                    ParamName = "Denomination",
                    ParamHelp = "Denomination value.",
                    ParamCreator = () => new integerParameter()
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.GamePlayDevice.DenomActiveParameterName,
                    ParamName = "Denom Active",
                    ParamHelp = "Indicates whether the denomination should be active.",
                    ParamCreator = () => new booleanParameter()
                }
            };

            var game = _games.GetGame(device.Id);

            var currentValues = game?.SupportedDenominations.Select(
                denom => new complexValue
                {
                    paramId = G2SParametersNames.GamePlayDevice.DenomListParameterName,
                    Items = new object[]
                    {
                        new integerValue1
                        {
                            paramId = G2SParametersNames.GamePlayDevice.DenomIdParameterName,
                            Value = denom
                        },
                        new booleanValue1
                        {
                            paramId = G2SParametersNames.GamePlayDevice.DenomActiveParameterName,
                            Value = game.ActiveDenominations.Contains(denom)
                        }
                    }
                });

            var defaultValues = game?.SupportedDenominations.Select(
                denom => new complexValue
                {
                    paramId = G2SParametersNames.GamePlayDevice.DenomListParameterName,
                    Items = new object[]
                    {
                        new integerValue1
                        {
                            paramId = G2SParametersNames.GamePlayDevice.DenomIdParameterName,
                            Value = denom
                        },
                        new booleanValue1
                        {
                            paramId = G2SParametersNames.GamePlayDevice.DenomActiveParameterName,
                            Value = false
                        }
                    }
                });

            return BuildOptionItemForTable(
                OptionConstants.GameDenomListOptionsId,
                t_securityLevels.G2S_operator,
                defaultValues.Count(),
                defaultValues.Count(),
                "G2S_denomList",
                "Denom List",
                "Identifies a list of active or inactive denominations",
                parameters,
                currentValues,
                defaultValues,
                includeDetails);
        }
    }
}