namespace Aristocrat.Monaco.G2S.Handlers.OptionConfig.Builders
{
    using System.Collections.Generic;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using Data.OptionConfig;

    /// <inheritdoc />
    public class OptionConfigDeviceOptionBuilder : BaseDeviceOptionBuilder<OptionConfigDevice>
    {
        /// <inheritdoc />
        protected override DeviceClass DeviceClass => DeviceClass.OptionConfig;

        /// <inheritdoc />
        protected override optionGroup[] BuildGroups(
            OptionConfigDevice device,
            OptionListCommandBuilderParameters parameters)
        {
            var group = new optionGroup
            {
                optionGroupId = "G2S_optionConfigOptions",
                optionGroupName = "G2S Option Config Options"
            };

            var items = new List<optionItem>();

            if (ShouldIncludeParam(OptionConstants.ProtocolOptionsId, parameters))
            {
                items.Add(BuildConfigProtocolOptions(device, parameters.IncludeDetails));
            }

            if (ShouldIncludeParam(OptionConstants.ProtocolAdditionalOptionsId, parameters))
            {
                items.Add(BuildProtocolAdditionalOptions(device, parameters.IncludeDetails));
            }

            group.optionItem = items.ToArray();

            return new[] { group };
        }

        private optionItem BuildConfigProtocolOptions(OptionConfigDevice device, bool includeDetails)
        {
            var parameters = new[]
            {
                ConfigIdParameter(device.ConfigurationId),
                NoResponseParameter(
                    device.NoResponseTimer,
                    "Max time to wait for an optionListAck response (0=disabled)",
                    OptionConfigDevice.DefaultNoResponseTimer)
            };

            return BuildOptionItem(
                OptionConstants.ProtocolOptionsId,
                t_securityLevels.G2S_administrator,
                1,
                1,
                "G2S_protocolParams",
                "G2S Protocol Parameters",
                "Standard G2S protocol parameters for option config devices",
                parameters,
                includeDetails);
        }
    }
}