namespace Aristocrat.Monaco.G2S.Handlers.NoteAcceptor
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Implementation of 'setNoteAcceptorState' command of 'NoteAcceptor' G2S class.
    /// </summary>
    public class SetNoteAcceptorState : ICommandHandler<noteAcceptor, setNoteAcceptorState>
    {
        private readonly ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus> _commandBuilder;
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SetNoteAcceptorState" /> class.
        /// </summary>
        public SetNoteAcceptorState(
            IG2SEgm egm,
            ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus> commandBuilder)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<noteAcceptor, setNoteAcceptorState> command)
        {
            return await Sanction.OnlyOwner<INoteAcceptorDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<noteAcceptor, setNoteAcceptorState> command)
        {
            var device = _egm.GetDevice<INoteAcceptorDevice>(command.IClass.deviceId);

            var enabled = command.Command.enable;

            if (device.HostEnabled != enabled)
            {
                device.DisableText = command.Command.disableText;
                device.HostEnabled = enabled;
            }

            var response = command.GenerateResponse<noteAcceptorStatus>();

            await _commandBuilder.Build(device, response.Command);
        }
    }
}