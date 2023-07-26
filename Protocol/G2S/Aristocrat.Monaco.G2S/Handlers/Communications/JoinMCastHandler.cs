namespace Aristocrat.Monaco.G2S.Handlers.Communications
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Communications;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Handles the v21.joinMcast G2S message
    /// </summary>
    public class JoinMCast : ICommandHandler<communications, joinMcast>
    {
        private readonly ICommandBuilder<ICommunicationsDevice, joinMcastAck> _commandBuilder;
        private readonly IEventLift _eventLift;
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="JoinMCast" /> class.
        ///     Creates a new instance of the JoinMCast handler
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="eventLift">A mechanism to raise a G2S event</param>
        /// <param name="commandBuilder">An <see cref="ICommandBuilder{TDevice,TCommand}" /> instance.</param>
        public JoinMCast(IG2SEgm egm, IEventLift eventLift, ICommandBuilder<ICommunicationsDevice, joinMcastAck> commandBuilder)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
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

                using(MessageBuilder messageBuilder = new MessageBuilder())
                {
                    messageBuilder.LoadSecurityNamespace(SchemaVersion.m105, null);
                    var securityParameters = messageBuilder.DecodeSecurityParams(command.Command.securityParams);

                    if (Uri.TryCreate(command.Command.multicastLocation, UriKind.Absolute, out Uri result))
                    {
                        device.CreateDeviceConnection(  command.Command.multicastId,
                                                        command.Command.deviceId,
                                                        command.IClass.deviceId, // This is the important ID for comms. This is the G2S Host ID which is also the G2S communications device Id.
                                                        "",
                                                        command.Command.multicastLocation,
                                                        EndpointUtilities.EncryptorKeyStringToArray(securityParameters.currentKey),
                                                        securityParameters.currentMsgId,
                                                        EndpointUtilities.EncryptorKeyStringToArray(securityParameters.newKey),
                                                        securityParameters.newKeyMsgId,
                                                        securityParameters.currentKeyLastMsgId);
                    }

                }

                _egm.StartMtp();

                _eventLift.Report(device, EventCode.G2S_CME110);

                var response = command.GenerateResponse<joinMcastAck>();

                await _commandBuilder.Build(device, response.Command);
            }
        }
    }
}