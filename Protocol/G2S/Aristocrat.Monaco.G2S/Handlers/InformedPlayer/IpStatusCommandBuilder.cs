namespace Aristocrat.Monaco.G2S.Handlers.InformedPlayer
{
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using System.Threading.Tasks;

    /// <summary>
    ///     A command builder for <see cref="ipStatus"/> response
    /// </summary>
    public class IpStatusCommandBuilder : ICommandBuilder<IInformedPlayerDevice, ipStatus>
    {
        /// <inheritdoc />
        public async Task Build(IInformedPlayerDevice device, ipStatus command)
        {
            command.configurationId = device.ConfigurationId;
            command.configDateTime = device.ConfigDateTime;
            command.configComplete = device.ConfigComplete;
            command.egmEnabled = device.Enabled;
            command.hostEnabled = device.HostEnabled;

            command.hostActive = device.HostActive;
            command.moneyInEnabled = device.MoneyInEnabled;
            command.gamePlayEnabled = device.GamePlayEnabled;
            command.sessionLimit = device.SessionLimit;

            await Task.CompletedTask;
        }
    }
}
