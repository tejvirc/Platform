namespace Aristocrat.Monaco.G2S.Handlers.Communications
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Handles the v21.joinMcast G2S message
    /// </summary>
    public class MCastKeyUpdate : ICommandHandler<communications, mcastKeyUpdate>
    {
        private readonly ICommandBuilder<ICommunicationsDevice, mcastKeyAck> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly MessageBuilder _messageBuilder = new MessageBuilder();

        /// <summary>
        ///     Initializes a new instance of the <see cref="MCastKeyUpdate" /> class.
        ///     Creates a new instance of the MCastKeyUpdate handler
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="commandBuilder">An <see cref="ICommandBuilder{TDevice,TCommand}" /> instance.</param>
        public MCastKeyUpdate(IG2SEgm egm, ICommandBuilder<ICommunicationsDevice, mcastKeyAck> commandBuilder)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<communications, mcastKeyUpdate> command)
        {
            return await Sanction.OnlyOwner<ICommunicationsDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<communications, mcastKeyUpdate> command)
        {
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                var device = _egm.GetDevice<ICommunicationsDevice>(command.IClass.deviceId);

                var sp = _messageBuilder.DecodeSecurityParams(command.Command.securityParams);

                device.SetMtpSecurityParameters(Encoding.ASCII.GetBytes(sp.currentKey),
                    sp.currentMsgId,
                    Encoding.ASCII.GetBytes(sp.newKey),
                    sp.currentKeyLastMsgId);

                // TODO: Handle errors

                _egm.StartMtp();

                /*
                 * 
                 * TODO where's the right place to raise up the G2S_CME110 event
                 * 
                */

                var response = command.GenerateResponse<mcastKeyAck>();

                await _commandBuilder.Build(device, response.Command);
            }
        }
    }
}