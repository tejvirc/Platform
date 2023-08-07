namespace Aristocrat.Monaco.G2S.Handlers.Communications
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Communications;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Handles the v21.mcastKeyUpdate G2S message
    /// </summary>
    public class MCastKeyUpdate : ICommandHandler<communications, mcastKeyUpdate>
    {
        private readonly ICommandBuilder<ICommunicationsDevice, mcastKeyAck> _commandBuilder;
        private readonly IG2SEgm _egm;

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

                using (MessageBuilder messageBuilder = new MessageBuilder())
                {
                    messageBuilder.LoadSecurityNamespace(SchemaVersion.m105, null);
                    var sp = messageBuilder.DecodeSecurityParams(command.Command.securityParams);
                    device.UpdateSecurityParameters( command.Command.multicastId,
                                                     EndpointUtilities.EncryptorKeyStringToArray(sp.currentKey),
                                                     sp.currentMsgId,
                                                     EndpointUtilities.EncryptorKeyStringToArray(sp.newKey),
                                                     sp.newKeyMsgId,
                                                     sp.currentKeyLastMsgId);
                }

                _egm.StartMtp();

                var response = command.GenerateResponse<mcastKeyAck>();

                await _commandBuilder.Build(device, response.Command);
            }
        }
    }
}