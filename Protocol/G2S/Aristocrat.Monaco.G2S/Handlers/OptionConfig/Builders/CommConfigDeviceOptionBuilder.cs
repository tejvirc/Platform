namespace Aristocrat.Monaco.G2S.Handlers.OptionConfig.Builders
{
    using System.Collections.Generic;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using Data.OptionConfig;

    /// <inheritdoc />
    public class CommConfigDeviceOptionBuilder : BaseDeviceOptionBuilder<CommConfigDevice>
    {
        /// <inheritdoc />
        protected override DeviceClass DeviceClass => DeviceClass.CommConfig;

        /// <inheritdoc />
        protected override optionGroup[] BuildGroups(
            CommConfigDevice device,
            OptionListCommandBuilderParameters parameters)
        {
            var group = new optionGroup
            {
                optionGroupId = "G2S_commConfigOptions",
                optionGroupName = "G2S Comm Config Options"
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

            group.optionItem = items.ToArray();

            return new[] { group };
        }

        private optionItem BuildProtocolOptions(ICommConfigDevice device, bool includeDetails)
        {
            var parameters = new[]
            {
                ConfigIdParameter(device.ConfigurationId),
                NoResponseParameter(
                    device.NoResponseTimer,
                    "Max time to wait for a commHostListAck response (0=disabled)",
                    CommConfigDevice.DefaultNoResponseTimer)
            };

            return BuildOptionItem(
                OptionConstants.ProtocolOptionsId,
                t_securityLevels.G2S_administrator,
                1,
                1,
                "G2S_protocolParams",
                "G2S Protocol Parameters",
                "Standard G2S protocol parameters for the comm config devices",
                parameters,
                includeDetails);
        }
    }
}