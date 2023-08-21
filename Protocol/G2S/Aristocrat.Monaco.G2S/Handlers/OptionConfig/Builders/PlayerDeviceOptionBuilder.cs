namespace Aristocrat.Monaco.G2S.Handlers.OptionConfig.Builders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using Data.OptionConfig;
    using Gaming.Contracts.Session;
    using Hardware.Contracts.IdReader;
    
    public class PlayerDeviceOptionBuilder : BaseDeviceOptionBuilder<PlayerDevice>
    {
        private readonly IIdReaderProvider _idReaderProvider;
        private readonly IPlayerService _players;

        public PlayerDeviceOptionBuilder(IPlayerService players, IIdReaderProvider idReaderProvider)
        {
            _players = players ?? throw new ArgumentNullException(nameof(players));
            _idReaderProvider = idReaderProvider ?? throw new ArgumentNullException(nameof(idReaderProvider));
        }

        protected override DeviceClass DeviceClass => DeviceClass.Player;

        protected override optionGroup[] BuildGroups(PlayerDevice device, OptionListCommandBuilderParameters parameters)
        {
            var optionGroups = new List<optionGroup>();

            var items = new List<optionItem>();

            var group = new optionGroup
            {
                optionGroupId = "G2S_playerOptions",
                optionGroupName = "G2S Player Options"
            };

            if (ShouldIncludeParam(OptionConstants.ProtocolOptionsId, parameters))
            {
                items.Add(
                    BuildProtocolOptions(
                        device,
                        device.RestartStatus,
                        "Standard G2S protocol parameters for player devices",
                        parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.PlayerOptionsId, parameters))
            {
                items.Add(BuildPlayerOptions(device, parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.PlayerSmdOptionsId, parameters))
            {
                items.Add(BuildPlayerSmdOptions(device, parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.MultipleDeviceOptionsId, parameters))
            {
                items.Add(BuildMultipleDeviceOptions(device, parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.PlayerValidationDeviceOptionsId, parameters))
            {
                items.Add(BuildValidationDeviceOptions(device, parameters.IncludeDetails));
            }

            group.optionItem = items.ToArray();
            optionGroups.Add(group);

            return optionGroups.ToArray();
        }

        private optionItem BuildPlayerOptions(IPlayerDevice device, bool includeDetails)
        {
            var parameters = new[]
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.PlayerDevice.DisplayPresentParameterName,
                    ParamName = "Display is present",
                    ParamHelp = "EGM can display messages",
                    ParamCreator = () => new booleanParameter { canModRemote = false, canModLocal = false },
                    ValueCreator = () => new booleanValue1(),
                    DefaultValue = true,
                    Value = device.DisplayPresent
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.PlayerDevice.IdReaderParameterName,
                    ParamName = "ID Reader to Use",
                    ParamHelp = "ID reader device to use for player tracking",
                    ParamCreator = () => new integerParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new integerValue1(),
                    DefaultValue = 0,
                    Value = device.IdReader
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.PlayerDevice.MinimumTheoHoldPercentageParameterName,
                    ParamName = "Minimum Theoretical Hold Percentage",
                    ParamHelp = "Minimum theoretical hold percentage (2 implied decimals)",
                    ParamCreator = () => new integerParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new integerValue1(),
                    DefaultValue = 0L,
                    Value = _players.Options.MinimumTheoreticalHoldPercentage
                },

                new ParameterDescription
                {
                    ParamId = G2SParametersNames.PlayerDevice.DecimalPointsParameterName,
                    ParamName = "Decimal Places",
                    ParamHelp = "Number of implied decimal places for point display",
                    ParamCreator = () => new integerParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new integerValue1(),
                    DefaultValue = 0,
                    Value = _players.Options.DecimalPoints
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.PlayerDevice.EndSessionOnInactivityParameterName,
                    ParamName = "End Session on inactivity",
                    ParamHelp = "Terminate session on inactivity",
                    ParamCreator = () => new booleanParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new booleanValue1(),
                    DefaultValue = true,
                    Value = _players.Options.InactiveSessionEnd
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.PlayerDevice.IntervalPeriodParameterName,
                    ParamName = "Time Interval Period",
                    ParamHelp = "How often to send interval ratings",
                    ParamCreator = () => new integerParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new integerValue1(),
                    DefaultValue = (int)TimeSpan.FromMinutes(10).TotalMilliseconds,
                    Value = (int)_players.Options.IntervalPeriod.TotalMilliseconds
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.PlayerDevice.GamePlayIntervalParameterName,
                    ParamName = "Game Play Period",
                    ParamHelp = "Generate interval ratings when selected game combo changes",
                    ParamCreator = () => new booleanParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new booleanValue1(),
                    DefaultValue = true,
                    Value = _players.Options.GamePlayInterval
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.PlayerDevice.MessageDurationParameterName,
                    ParamName = "Message Duration",
                    ParamHelp = "Message duration (in milliseconds)",
                    ParamCreator = () => new integerParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new integerValue1(),
                    DefaultValue = PlayerDevice.MessageDurationDefault,
                    Value = device.MessageDuration
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.PlayerDevice.CountBasisParameterName,
                    ParamName = "Countdown Basis",
                    ParamHelp = "Meter basis used for countdown calculation",
                    ParamCreator = () => new stringParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new stringValue1(),
                    DefaultValue = string.Empty,
                    Value = _players.Options.CountBasis
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.PlayerDevice.CountDirectionParameterName,
                    ParamName = "Count Direction",
                    ParamHelp = "Count direction (up or down)",
                    ParamCreator = () => new stringParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new stringValue1(),
                    DefaultValue = t_countDirection.G2S_down.ToString(),
                    Value = (_players.Options.CountDirection == CountDirection.Up
                        ? t_countDirection.G2S_up
                        : t_countDirection.G2S_down).ToString()
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.PlayerDevice.BaseTargetParameterName,
                    ParamName = "Base Countdown Target",
                    ParamHelp = "Countdown value needed to earn a point",
                    ParamCreator = () => new integerParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new integerValue1(),
                    DefaultValue = 0,
                    Value = _players.Options.BaseTarget
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.PlayerDevice.BaseIncrementParameterName,
                    ParamName = "Base Countdown Increment",
                    ParamHelp = "Value of basis change (in units of countBasis) needed to change the countdown",
                    ParamCreator = () => new integerParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new integerValue1(),
                    DefaultValue = 0L,
                    Value = _players.Options.BaseIncrement
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.PlayerDevice.BaseAwardParameterName,
                    ParamName = "Base Countdown Award",
                    ParamHelp = "How many points are awarded when countdown target is achieved",
                    ParamCreator = () => new integerParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new integerValue1(),
                    DefaultValue = 0,
                    Value = _players.Options.BaseAward
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.PlayerDevice.HotPlayerBasisParameterName,
                    ParamName = "Hot Player Basis",
                    ParamHelp = "Meter basis used for hot player calculation",
                    ParamCreator = () => new stringParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new stringValue1(),
                    DefaultValue = string.Empty,
                    Value = _players.Options.HotPlayerBasis
                },

                new ParameterDescription
                {
                    ParamId = G2SParametersNames.PlayerDevice.HotPlayerPeriodParameterName,
                    ParamName = "Hot Player Period",
                    ParamHelp = "Meter basis used for hot player calculation",
                    ParamCreator = () => new integerParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new integerValue1(),
                    DefaultValue = (int)TimeSpan.FromMinutes(10).TotalMilliseconds,
                    Value = (int)_players.Options.HotPlayerPeriod.TotalMilliseconds
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.PlayerDevice.HotPlayerLimit1ParameterName,
                    ParamName = "Hot Player Limit 1",
                    ParamHelp = "Value of play during period to achieve Hot Player Limit 1 (0 = inactive)",
                    ParamCreator = () => new integerParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new integerValue1(),
                    DefaultValue = 0L,
                    Value = _players.Options.HotPlayerLimit1
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.PlayerDevice.HotPlayerLimit2ParameterName,
                    ParamName = "Hot Player Limit 2",
                    ParamHelp = "Value of play during period to achieve Hot Player Limit 2 (0 = inactive)",
                    ParamCreator = () => new integerParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new integerValue1(),
                    DefaultValue = 0L,
                    Value = _players.Options.HotPlayerLimit2
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.PlayerDevice.HotPlayerLimit3ParameterName,
                    ParamName = "Hot Player Limit 3",
                    ParamHelp = "Value of play during period to achieve Hot Player Limit 3 (0 = inactive)",
                    ParamCreator = () => new integerParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new integerValue1(),
                    DefaultValue = 0L,
                    Value = _players.Options.HotPlayerLimit3
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.PlayerDevice.HotPlayerLimit4ParameterName,
                    ParamName = "Hot Player Limit 4",
                    ParamHelp = "Value of play during period to achieve Hot Player Limit 4 (0 = inactive)",
                    ParamCreator = () => new integerParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new integerValue1(),
                    DefaultValue = 0L,
                    Value = _players.Options.HotPlayerLimit4
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.PlayerDevice.HotPlayerLimit5ParameterName,
                    ParamName = "Hot Player Limit 5",
                    ParamHelp = "Value of play during period to achieve Hot Player Limit 5 (0 = inactive)",
                    ParamCreator = () => new integerParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new integerValue1(),
                    DefaultValue = 0L,
                    Value = _players.Options.HotPlayerLimit5
                }
            };

            return BuildOptionItem(
                OptionConstants.PlayerOptionsId,
                t_securityLevels.G2S_attendant,
                1,
                1,
                "G2S_playerParams",
                "Player Options",
                "Configuration Parameters for this Player device",
                parameters,
                includeDetails);
        }

        private optionItem BuildPlayerSmdOptions(PlayerDevice device, bool includeDetails)
        {
            var parameters = new[]
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.PlayerDevice.SendMeterDeltaParameterName,
                    ParamName = "Send Meter Deltas",
                    ParamHelp =
                        "Indicates whether the EGM must send the meter delta information when a player session ends.",
                    ParamCreator = () => new booleanParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new booleanValue1(),
                    DefaultValue = false,
                    Value = device.SendMeterDelta
                }
            };

            return BuildOptionItem(
                OptionConstants.PlayerSmdOptionsId,
                t_securityLevels.G2S_attendant,
                1,
                1,
                "G2S_protocolParamsSMD",
                "Extended Protocol Parameters",
                "Configuration settings for Meter Delta Logs",
                parameters,
                includeDetails);
        }

        private optionItem BuildMultipleDeviceOptions(PlayerDevice device, bool includeDetails)
        {
            var parameters = new[]
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.PlayerDevice.MultipleValidationDevicesParameterName,
                    ParamName = "Use Multiple ID Devices",
                    ParamHelp = "Indicates whether multiple ID readers can be used for validating player Ids.",
                    ParamCreator = () => new booleanParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new booleanValue1(),
                    DefaultValue = false,
                    Value = device.UseMultipleIdDevices
                }
            };

            return BuildOptionItem(
                OptionConstants.MultipleDeviceOptionsId,
                t_securityLevels.G2S_operator,
                1,
                1,
                "G2S_multipleValDeviceParams",
                "Multiple Validation Device Parameters",
                "Multiple validation devices for players",
                parameters,
                includeDetails);
        }

        private optionItem BuildValidationDeviceOptions(PlayerDevice device, bool includeDetails)
        {
            var parameters = new[]
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.PlayerDevice.IdReaderTypeParameterName,
                    ParamName = "ID Reader Type",
                    ParamHelp = "ID reader type.",
                    ParamCreator = () => new stringParameter()
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.PlayerDevice.IdReaderIdParameterName,
                    ParamName = "ID Reader Device",
                    ParamHelp = "ID reader device identifier.",
                    ParamCreator = () => new integerParameter()
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.PlayerDevice.IdReaderLinkedParameterName,
                    ParamName = "Linked",
                    ParamHelp = "Indicates whether the ID reader is linked to the player device.",
                    ParamCreator = () => new booleanParameter { canModRemote = true, canModLocal = true }
                }
            };

            var idReaders = _idReaderProvider.Adapters;

            var currentValues = idReaders.Select(
                reader => new complexValue
                {
                    paramId = G2SParametersNames.PlayerDevice.PlayerValidationDeviceParameterName,
                    Items = new object[]
                    {
                        new stringValue1
                        {
                            paramId = G2SParametersNames.PlayerDevice.IdReaderTypeParameterName,
                            Value = reader.IdReaderType.ToIdReaderType()
                        },
                        new integerValue1
                        {
                            paramId = G2SParametersNames.PlayerDevice.IdReaderIdParameterName,
                            Value = reader.IdReaderId
                        },
                        new booleanValue1
                        {
                            paramId = G2SParametersNames.PlayerDevice.IdReaderLinkedParameterName,
                            Value = device.IdReader == reader.IdReaderId ||
                                    device.IdReaders.Any(id => id == reader.IdReaderId)
                        }
                    }
                });

            return BuildOptionItemForTable(
                OptionConstants.PlayerValidationDeviceOptionsId,
                t_securityLevels.G2S_operator,
                0,
                9,
                G2SParametersNames.PlayerDevice.PlayerValidationDeviceParameterName,
                "Additional Validation Devices",
                "Additional validation devices for players",
                parameters,
                currentValues,
                Enumerable.Empty<complexValue>(),
                includeDetails);
        }
    }
}
