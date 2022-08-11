namespace Aristocrat.Monaco.G2S.Handlers.Communications
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Handles the v21.joinMcast G2S message
    /// </summary>
    public class JoinMCast : ICommandHandler<communications, joinMcast>
    {
        private readonly ICommandBuilder<ICommunicationsDevice, joinMcastAck> _commandBuilder;
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="JoinMCast" /> class.
        ///     Creates a new instance of the JoinMCast handler
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="commandBuilder">An <see cref="ICommandBuilder{TDevice,TCommand}" /> instance.</param>
        public JoinMCast(IG2SEgm egm, ICommandBuilder<ICommunicationsDevice, joinMcastAck> commandBuilder)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<communications, joinMcast> command)
        {
            return await Sanction.OnlyOwner<ICommunicationsDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<communications, joinMcast> command)
        {
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                var device = _egm.GetDevice<ICommunicationsDevice>(command.IClass.deviceId);

                /*
                 * 
                 * JLK: Need to trigger instantiation of the mtpClient in G2SEgm here, and connect to the multicast group
                 * Then it needs to raise up the G2S_CME110 event
                 * Look through the other handlers and find one that's long-lived
                 * 
                 * 
                device.TriggerStateChange(
                    command.Command.enable
                        ? CommunicationTrigger.HostEnabled
                        : CommunicationTrigger.HostDisabled,
                    command.Command.disableText);
                */
                var response = command.GenerateResponse<joinMcastAck>();

                await _commandBuilder.Build(device, response.Command);
            }
        }
    }
}