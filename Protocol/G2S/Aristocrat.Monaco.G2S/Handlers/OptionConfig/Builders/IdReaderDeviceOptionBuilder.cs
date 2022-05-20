namespace Aristocrat.Monaco.G2S.Handlers.OptionConfig.Builders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using Data.OptionConfig;
    using Hardware.Contracts.IdReader;
    using IdReader;

    /// <inheritdoc />
    public class IdReaderDeviceOptionBuilder :
        BaseDeviceOptionBuilder<IdReaderDevice>
    {
        private readonly IIdReaderProvider _provider;

        public IdReaderDeviceOptionBuilder(IIdReaderProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        /// <inheritdoc />
        protected override DeviceClass DeviceClass => DeviceClass.IdReader;

        /// <inheritdoc />
        protected override optionGroup[] BuildGroups(
            IdReaderDevice device,
            OptionListCommandBuilderParameters parameters)
        {
            var group = new optionGroup
            {
                optionGroupId = OptionConstants.IdReaderOptionsId,
                optionGroupName = "G2S ID Reader Options"
            };

            var items = new List<optionItem>();

            if (ShouldIncludeParam(OptionConstants.ProtocolOptionsId, parameters))
            {
                var additionalParameters = BuildAdditionalProtocolParameters(device);

                items.Add(
                    BuildProtocolOptions(
                        device,
                        device.RestartStatus,
                        "Standard G2S protocol parameters for ID reader devices",
                        additionalParameters,
                        parameters.IncludeDetails)
                );
            }

            if (ShouldIncludeParam(OptionConstants.IdReaderOptionsId, parameters))
            {
                items.Add(BuildIdReaderOptions(device, parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.IdReaderMessagesId, parameters))
            {
                items.Add(BuildIdReaderMessages(device, parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.IdReaderPatternTable, parameters))
            {
                items.Add(BuildIdReaderPatternTable(device, parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.ProtocolAdditionalOptionsId, parameters))
            {
                items.Add(BuildProtocolAdditionalOptions(device, parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.IdReaderLocaleTable, parameters))
            {
                //items.Add(BuildIdReaderLocaleTable(device, parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.IdReaderNoPlayerMessageOptionId, parameters))
            {
                items.Add(BuildIdReaderNoPlayerMessageOption(device, parameters.IncludeDetails));
            }

            group.optionItem = items.ToArray();

            return new[] { group };
        }

        /// <summary>
        ///     Apply the "Time to Live" sub-parameter from the ID Reader.
        /// </summary>
        /// <param name="device">The ID Reader</param>
        /// <returns>The sub-parameter "Time to Live" </returns>
        private IEnumerable<ParameterDescription> BuildAdditionalProtocolParameters(IIdReaderDevice device)
        {
            return new List<ParameterDescription>
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
                }
            };
        }

        private optionItem BuildIdReaderOptions(IIdReaderDevice device, bool includeDetails)
        {
            var idReader = _provider[device.Id];

            // Fill in all the sub-parameter definitions of the Device Option Configuration G2S spec section
            var parameters = new[]
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.IdReaderDevice.EgmPhysicallyControlsParameterName,
                    ParamName = "EGM Controls Reader", // paramName
                    ParamHelp = "EGM controls reader", // paramHelp
                    ParamCreator = () => new booleanParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new booleanValue1(),
                    DefaultValue = false,
                    Value = idReader.IsEgmControlled
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.IdReaderDevice.IdReaderTypeParameterName,
                    ParamName = "ID Reader Type", // paramName
                    ParamHelp = "ID reader type", // paramHelp
                    ParamCreator = () => new stringParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new stringValue1(),
                    DefaultValue = "G2S_none",
                    Value = idReader.IdReaderType.ToIdReaderType()
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.IdReaderDevice.IdReaderTrackParameterName,
                    ParamName = "Read From Track", // paramName
                    ParamHelp = "Read from track", // paramHelp
                    ParamCreator = () => new integerParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new integerValue1(),
                    DefaultValue = 1,
                    Value = idReader.IdReaderTrack.ToTrackId()
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.IdReaderDevice.IdValidMethodParameterName,
                    ParamName = "ID Validation Method", // paramName
                    ParamHelp = "ID validation method", // paramHelp
                    ParamCreator = () => new stringParameter { canModRemote = false, canModLocal = true },
                    ValueCreator = () => new stringValue1(),
                    DefaultValue = t_idValidMethods.G2S_host.ToString(),
                    Value = idReader.ValidationMethod.ToValidationMethod().ToString()
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.IdReaderDevice.WaitTimeOutParameterName,
                    ParamName = "Wait Timeout", // paramName
                    ParamHelp = "Wait timeout (in milliseconds)", // paramHelp
                    ParamCreator = () => new integerParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new integerValue1(),
                    DefaultValue = 300000,
                    Value = idReader.WaitTimeout
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.IdReaderDevice.OffLineValidParameterName,
                    ParamName = "Do Offline Validation", // paramName
                    ParamHelp = "Do offline validation", // paramHelp
                    ParamCreator = () => new booleanParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new booleanValue1(),
                    DefaultValue = false,
                    Value = idReader.SupportsOfflineValidation
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.IdReaderDevice.ValidTimeOutParameterName,
                    ParamName = "Abandoned Card Timer", // paramName
                    ParamHelp = "Abandoned card timer (in milliseconds)", // paramHelp
                    ParamCreator = () => new integerParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new integerValue1(),
                    DefaultValue = 300000,
                    Value = idReader.ValidationTimeout
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.IdReaderDevice.RemovalDelayParameterName,
                    ParamName = "ID Removal Delay", // paramName
                    ParamHelp = "ID removal delay (in milliseconds)", // paramHelp
                    ParamCreator = () => new integerParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new integerValue1(),
                    DefaultValue = IdReaderDevice.DefaultRemovalDelayParameterName,
                    Value = device.RemovalDelay
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.IdReaderDevice.MsgDurationParameterName,
                    ParamName = "Message Duration", // paramName
                    ParamHelp = "Message duration (in milliseconds)", // paramHelp
                    ParamCreator = () => new integerParameter { canModRemote = true, canModLocal = true, minIncl = 0 },
                    ValueCreator = () => new integerValue1(),
                    DefaultValue = IdReaderDevice.DefaultMsgDuration,
                    Value = device.MsgDuration
                }
            };

            return BuildOptionItem(
                OptionConstants.IdReaderOptionsId, // 'optionId' in the G2S spec
                t_securityLevels.G2S_operator, // 'securityLevel' in the G2S spec
                1, // 'minSelections' in the G2S spec
                1, // 'maxSelections' in the G2S spec
                "G2S_idReaderParams", // 'paramId' in the G2S spec
                "ID Reader Options", // 'paramName' in the G2S spec
                "Configuration parameters for this ID reader device", // 'paramHelp' in the G2S spec
                parameters,
                includeDetails);
        }

        private optionItem BuildIdReaderMessages(IIdReaderDevice device, bool includeDetails)
        {
            var parameters = new[]
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.IdReaderDevice.AttractMsgParameterName,
                    ParamName = "Attract Message", // paramName
                    ParamHelp = "Message to display when no ID is present", // paramHelp
                    ParamCreator = () => new stringParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new stringValue1(),
                    DefaultValue = IdReaderDevice.DefaultAttractMsg,
                    Value = device.AttractMsg
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.IdReaderDevice.WaitMsgParameterName,
                    ParamName = "Wait Message", // paramName
                    ParamHelp = "Message to display while waiting for validation", // paramHelp
                    ParamCreator = () => new stringParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new stringValue1(),
                    DefaultValue = IdReaderDevice.DefaultWaitMsg,
                    Value = device.WaitMsg
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.IdReaderDevice.ValidMsgParameterName,
                    ParamName = "Valid ID Message", // paramName
                    ParamHelp = "Message to display while a valid ID is present", // paramHelp
                    ParamCreator = () => new stringParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new stringValue1(),
                    DefaultValue = IdReaderDevice.DefaultValidMsg,
                    Value = device.ValidMsg
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.IdReaderDevice.InvalidMsgParameterName,
                    ParamName = "Invalid ID Message", // paramName
                    ParamHelp = "Message to display while an invalid ID is present", // paramHelp
                    ParamCreator = () => new stringParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new stringValue1(),
                    DefaultValue = IdReaderDevice.DefaultInvalidMsg,
                    Value = device.InvalidMsg
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.IdReaderDevice.LostMsgParameterName,
                    ParamName = "Lost ID Message", // paramName
                    ParamHelp = "Message to display while a lost ID is present", // paramHelp
                    ParamCreator = () => new stringParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new stringValue1(),
                    DefaultValue = IdReaderDevice.DefaultLostMsg,
                    Value = device.LostMsg
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.IdReaderDevice.OffLineMsgParameterName,
                    ParamName = "Offline Message", // paramName
                    ParamHelp = "Message to display if an ID cannot be validated", // paramHelp
                    ParamCreator = () => new stringParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new stringValue1(),
                    DefaultValue = IdReaderDevice.DefaultOffLineMsg,
                    Value = device.OffLineMsg
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.IdReaderDevice.AbandonMsgParameterName,
                    ParamName = "Abandoned Message", // paramName
                    ParamHelp = "Message to display if an ID is abandoned", // paramHelp
                    ParamCreator = () => new stringParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new stringValue1(),
                    DefaultValue = IdReaderDevice.DefaultAbandonMsg,
                    Value = device.AbandonMsg
                }
            };

            return BuildOptionItem(
                OptionConstants.IdReaderMessagesId, // 'optionId' in the G2S spec
                t_securityLevels.G2S_operator, // 'securityLevel' in the G2S spec
                1, // 'minSelections' in the G2S spec
                1, // 'maxSelections' in the G2S spec
                "G2S_messageList", // 'paramId' in the G2S spec
                "ID Reader Message List", // 'paramName' in the G2S spec
                "Messages to be used by this ID reader device", // 'paramHelp' in the G2S spec
                parameters,
                includeDetails);
        }

        private optionItem BuildIdReaderPatternTable(IIdReaderDevice device, bool includeDetails)
        {
            var idReader = _provider[device.Id];

            var idTypes = Enum.GetValues(typeof(IdTypes));

            var patterns = idReader.Patterns.Select(
                pattern =>
                    new complexValue
                    {
                        paramId = "G2S_idPattern",
                        Items = new object[]
                        {
                            new stringValue1
                            {
                                paramId = G2SParametersNames.IdReaderDevice.IdTypeParameterName,
                                Value = pattern.IdType
                            },
                            new stringValue1
                            {
                                paramId = G2SParametersNames.IdReaderDevice.OffLinePatternParameterName,
                                Value = pattern.OfflinePattern
                            }
                        }
                    }
                );

            var parameters = new List<ParameterDescription>
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.IdReaderDevice.IdTypeParameterName,
                    ParamName = "ID Type",
                    ParamHelp = "ID type",
                    ParamCreator = () => new integerParameter { canModLocal = true, canModRemote = true, paramKey = true }
                    
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.IdReaderDevice.OffLinePatternParameterName,
                    ParamName = "ID Pattern",
                    ParamHelp = "Expected pattern for this ID type",
                    ParamCreator = () => new stringParameter { canModLocal = true, canModRemote = true }
                }
            };

            return BuildOptionItemForTable(
                OptionConstants.IdReaderPatternTable,
                t_securityLevels.G2S_operator,
                0,
                idTypes.Length,
                "G2S_idPattern",
                "Offline Validation Pattern",
                "Pattern match when offline to determine ID type",
                parameters,
                patterns,
                Enumerable.Empty<complexValue>(),
                includeDetails);
        }

        private optionItem BuildIdReaderNoPlayerMessageOption(IIdReaderDevice device, bool includeDetails)
        {
            var parameters = new[]
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.IdReaderDevice.NoPlayerMessagesParameterName,
                    ParamName = "Disable Player Tracking Messages", // paramName
                    ParamHelp =
                        "Indicates whether a host can disable player tracking messages for a specific player.", // paramHelp
                    ParamCreator = () => new booleanParameter { canModRemote = true, canModLocal = true },
                    ValueCreator = () => new booleanValue1(),
                    DefaultValue = IdReaderDevice.DefaultNoPlayerMessages,
                    Value = device.NoPlayerMessages
                }
            };

            return BuildOptionItem(
                OptionConstants.IdReaderNoPlayerMessageOptionId, // 'optionId' in the G2S spec
                t_securityLevels.G2S_operator, // 'securityLevel' in the G2S spec
                1, // 'minSelections' in the G2S spec
                1, // 'maxSelections' in the G2S spec
                "G2S_noPlayerMessagesParams", // 'paramId' in the G2S spec
                "Disable Player Tracking Messages Parameters", // 'paramName' in the G2S spec
                "Allows a host to disable player tracking messages for specific players.", // 'paramHelp' in the G2S spec
                parameters,
                includeDetails);
        }
    }
}
