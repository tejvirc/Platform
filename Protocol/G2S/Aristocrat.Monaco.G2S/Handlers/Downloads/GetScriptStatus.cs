namespace Aristocrat.Monaco.G2S.Handlers.Downloads
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Handles the v21.getScriptStatus G2S message
    /// </summary>
    public class GetScriptStatus : ICommandHandler<download, getScriptStatus>
    {
        private readonly ICommandBuilder<IDownloadDevice, scriptStatus> _command;
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetScriptStatus" /> class.
        ///     Creates a new instance of the SetScriptStatus handler
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="command">Script status command builder.</param>
        public GetScriptStatus(IG2SEgm egm, ICommandBuilder<IDownloadDevice, scriptStatus> command)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _command = command ?? throw new ArgumentNullException(nameof(command));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<download, getScriptStatus> command)
        {
            return await Sanction.OwnerAndGuests<IDownloadDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<download, getScriptStatus> command)
        {
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                var dl = _egm.GetDevice<IDownloadDevice>(command.IClass.deviceId);
                if (dl == null)
                {
                    return;
                }

                var response = command.GenerateResponse<scriptStatus>();
                response.Command.scriptId = command.Command.scriptId;
                await _command.Build(dl, response.Command);

                if (response.Command.scriptId == 0)
                {
                    command.Error.Code = ErrorCode.G2S_DLX005;
                }
            }

            await Task.CompletedTask;
        }
    }
}