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
    ///     Handles the v21.setCommsState G2S message
    /// </summary>
    public class SetCommsState : ICommandHandler<communications, setCommsState>
    {
        private readonly ICommandBuilder<ICommunicationsDevice, commsStatus> _commandBuilder;
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SetCommsState" /> class.
        ///     Creates a new instance of the SetCommsState handler
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="commandBuilder">An <see cref="ICommandBuilder{TDevice,TCommand}" /> instance.</param>
        public SetCommsState(IG2SEgm egm, ICommandBuilder<ICommunicationsDevice, commsStatus> commandBuilder)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<communications, setCommsState> command)
        {
            return await Sanction.OnlyOwner<ICommunicationsDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<communications, setCommsState> command)
        {
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                var device = _egm.GetDevice<ICommunicationsDevice>(command.IClass.deviceId);

                device.TriggerStateChange(
                    command.Command.enable
                        ? CommunicationTrigger.HostEnabled
                        : CommunicationTrigger.HostDisabled,
                    command.Command.disableText);

                var response = command.GenerateResponse<commsStatus>();

                await _commandBuilder.Build(device, response.Command);
            }
        }
    }
}