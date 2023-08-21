namespace Aristocrat.Monaco.G2S.Handlers.Communications
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     An implementation of <see cref="ICommandBuilder{TDevice,TCommand}" />
    /// </summary>
    public class CommsStatusCommandBuilder : ICommandBuilder<ICommunicationsDevice, commsStatus>
    {
        /// <inheritdoc />
        public async Task Build(ICommunicationsDevice device, commsStatus command)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            command.configurationId = device.ConfigurationId;
            command.hostEnabled = device.HostEnabled;
            command.egmEnabled = device.Enabled;
            command.outboundOverflow = device.OutboundOverflow;
            command.inboundOverflow = device.InboundOverflow;
            command.g2sProtocol = true;
            command.commsState = device.State;
            command.transportState = device.TransportState.ToString();

            await Task.CompletedTask;
        }
    }
}