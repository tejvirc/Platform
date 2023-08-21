namespace Aristocrat.Monaco.G2S.Handlers.CoinAcceptor
{
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     An implementation of <see cref="ICommandBuilder{TDevice,TCommand}" />
    /// </summary>
    public class CoinAcceptorStatusCommandBuilder : ICommandBuilder<ICoinAcceptor, coinAcceptorStatus>
    {
        /// <inheritdoc />
        public async Task Build(ICoinAcceptor device, coinAcceptorStatus command)
        {
            command.configurationId = device.ConfigurationId;
            command.hostEnabled = device.HostEnabled;
            command.egmEnabled = device.Enabled;
            command.disconnected = false;
            command.firmwareFault = false;
            command.mechanicalFault = false;
            command.mechanicalFault = false;
            command.opticalFault = false;
            command.nvMemoryFault = false;
            command.illegalActivity = false;
            command.doorOpen = false;
            command.jammed = false;
            command.lockoutMalfunction = false;
            command.acceptorFault = false;
            command.diverterFault = false;
            command.configComplete = device.ConfigComplete;
            command.configDateTime = device.ConfigDateTime;

            await Task.CompletedTask;
        }
    }
}