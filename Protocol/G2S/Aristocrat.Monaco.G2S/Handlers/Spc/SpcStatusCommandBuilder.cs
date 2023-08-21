namespace Aristocrat.Monaco.G2S.Handlers.Spc
{
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    public class SpcStatusCommandBuilder : ICommandBuilder<ISpcDevice, spcStatus>
    {
        public async Task Build(ISpcDevice device, spcStatus command)
        {
            command.configurationId = device.ConfigurationId;
            command.hostEnabled = device.HostEnabled;
            command.egmEnabled = device.Enabled;
            command.configDateTime = device.ConfigDateTime;
            command.configComplete = device.ConfigComplete;
            command.spcLevelStatus = Enumerable.Empty<spcLevelStatus>().ToArray();

            await Task.CompletedTask;
        }
    }
}
