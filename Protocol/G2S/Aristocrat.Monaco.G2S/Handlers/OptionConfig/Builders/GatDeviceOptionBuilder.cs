namespace Aristocrat.Monaco.G2S.Handlers.OptionConfig.Builders
{
    using System.Collections.Generic;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using Data.OptionConfig;
    using Linq;

    /// <inheritdoc />
    public class GatDeviceOptionBuilder : BaseDeviceOptionBuilder<GatDevice>
    {
        /// <inheritdoc />
        protected override DeviceClass DeviceClass => DeviceClass.Gat;

        /// <inheritdoc />
        protected override optionGroup[] BuildGroups(GatDevice device, OptionListCommandBuilderParameters parameters)
        {
            var group = new optionGroup
            {
                optionGroupId = "G2S_gatOptions",
                optionGroupName = "G2S GAT Options"
            };

            var items = new List<optionItem>();

            if (ShouldIncludeParam(OptionConstants.ProtocolOptionsId, parameters))
            {
                items.Add(BuildProtocolOptions(device, parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.ProtocolAdditionalOptionsId, parameters))
            {
                items.Add(BuildProtocolAdditionalOptions(device, parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.GatSpecialFunctionsOptionsId, parameters))
            {
                items.Add(BuildSpecialFunctionsOptions(device, parameters.IncludeDetails));
            }

            group.optionItem = items.ToArray();

            return new[] { group };
        }

        private optionItem BuildProtocolOptions(IGatDevice device, bool includeDetails)
        {
            var parameters = new[]
            {
                ConfigIdParameter(device.ConfigurationId),
                UseDefaultConfigParameter(device.UseDefaultConfig),
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
                    ParamId = G2SParametersNames.GatDevice.IdReaderIdParameterName,
                    ParamName = "ID Reader to Use",
                    ParamHelp = "ID reader to use for this GAT device",
                    ParamCreator = () => new integerParameter { canModRemote = true },
                    ValueCreator = () => new integerValue1(),
                    Value = device.IdReaderId,
                    DefaultValue = 0
                },
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.GatDevice.MinQueuedCompsParameterName,
                    ParamName = "Minimum Queued Component Requests",
                    ParamHelp = "Minimum number of components that the EGM must be able to queue for verification",
                    ParamCreator = () => new integerParameter { canModRemote = true },
                    ValueCreator = () => new integerValue1(),
                    Value = device.MinQueuedComps,
                    DefaultValue = 0
                }
            };

            return BuildOptionItem(
                OptionConstants.ProtocolOptionsId,
                t_securityLevels.G2S_administrator,
                1,
                1,
                "G2S_protocolParams",
                "G2S Protocol Parameters",
                "Standard G2S protocol parameters for GAT devices",
                parameters,
                includeDetails);
        }

        private optionItem BuildSpecialFunctionsOptions(IGatDevice device, bool includeDetails)
        {
            var parameters = new[]
            {
                new ParameterDescription
                {
                    ParamId = G2SParametersNames.GatDevice.SpecialFunctionsParameterName,
                    ParamName = "Special Functions",
                    ParamHelp = "Indicates whether special function commands should be processed.",
                    ParamCreator = () => new stringParameter { canModRemote = true },
                    ValueCreator = () => new stringValue1(),
                    Value = device.SpecialFunctions.ToName(),
                    DefaultValue = GatDevice.DefaultSpecialFunctions.ToName()
                }
            };

            return BuildOptionItem(
                OptionConstants.GatSpecialFunctionsOptionsId,
                t_securityLevels.G2S_administrator,
                1,
                1,
                "G2S_specialFunctionsParams",
                "GAT Special Functions Parameters",
                "Parameters for enabling GAT special functions.",
                parameters,
                includeDetails);
        }
    }
}