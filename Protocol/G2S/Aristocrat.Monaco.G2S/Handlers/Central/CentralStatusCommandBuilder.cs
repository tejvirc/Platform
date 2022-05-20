namespace Aristocrat.Monaco.G2S.Handlers.Central
{
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    public class CentralStatusCommandBuilder : ICommandBuilder<ICentralDevice, centralStatus>
    {
        public Task Build(ICentralDevice device, centralStatus command)
        {
            command.configurationId = device.ConfigurationId;
            command.hostEnabled = device.HostEnabled;
            command.egmEnabled = device.Enabled;
            command.configDateTime = device.ConfigDateTime;
            command.configComplete = device.ConfigComplete;

            return Task.CompletedTask;
        }
    }
}