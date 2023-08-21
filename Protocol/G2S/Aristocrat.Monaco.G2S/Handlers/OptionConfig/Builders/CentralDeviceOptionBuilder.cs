namespace Aristocrat.Monaco.G2S.Handlers.OptionConfig.Builders
{
    using System.Collections.Generic;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using Data.OptionConfig;

    public class CentralDeviceOptionBuilder : BaseDeviceOptionBuilder<CentralDevice>
    {
        protected override DeviceClass DeviceClass => DeviceClass.Central;

        protected override optionGroup[] BuildGroups(
            CentralDevice device,
            OptionListCommandBuilderParameters parameters)
        {
            var items = new List<optionItem>();

            var group = new optionGroup
            {
                optionGroupId = "G2S_centralOptions", optionGroupName = "G2S Central Options"
            };

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
                    NoResponseParameter(
                        device.NoResponseTimer,
                        "Max time to wait for a message response (in milliseconds) 0 = disabled",
                        DownloadDevice.DefaultNoResponseTimer)
                };

                items.Add(
                    BuildProtocolOptions(
                        device,
                        device.RestartStatus,
                        "Standard G2S protocol parameters for central devices",
                        additionalParameters,
                        parameters.IncludeDetails));
            }

            group.optionItem = items.ToArray();

            return new[] { group };
        }
    }
}