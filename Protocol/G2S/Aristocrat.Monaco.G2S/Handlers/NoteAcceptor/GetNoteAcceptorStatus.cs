namespace Aristocrat.Monaco.G2S.Handlers.NoteAcceptor
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Implementation of 'getNoteAcceptorStatus' command of 'NoteAcceptor' G2S class.
    /// </summary>
    public class GetNoteAcceptorStatus : ICommandHandler<noteAcceptor, getNoteAcceptorStatus>
    {
        private readonly ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus> _command;
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetNoteAcceptorStatus" /> class.
        /// </summary>
        /// <param name="egm">The G2S egm.</param>
        /// <param name="command">An <see cref="ICommandBuilder{INoteAcceptorDevice,noteAcceptorStatus}" /> implementation</param>
        public GetNoteAcceptorStatus(IG2SEgm egm, ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus> command)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _command = command ?? throw new ArgumentNullException(nameof(command));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<noteAcceptor, getNoteAcceptorStatus> command)
        {
            return await Sanction.OwnerAndGuests<INoteAcceptorDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<noteAcceptor, getNoteAcceptorStatus> command)
        {
            if (command.Class.sessionType != t_sessionTypes.G2S_request)
            {
                return;
            }

            var device = _egm.GetDevice<INoteAcceptorDevice>(command.IClass.deviceId);
            var response = command.GenerateResponse<noteAcceptorStatus>();

            await _command.Build(device, response.Command);
        }
    }
}