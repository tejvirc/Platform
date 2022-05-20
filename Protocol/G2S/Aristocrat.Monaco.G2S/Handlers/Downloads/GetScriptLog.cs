namespace Aristocrat.Monaco.G2S.Handlers.Downloads
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Common.PackageManager.Storage;
    using Monaco.Common.Storage;

    /// <summary>
    ///     Handles the v21.getScriptLog G2S message
    /// </summary>
    public class GetScriptLog : ICommandHandler<download, getScriptLog>
    {
        private readonly ICommandBuilder<IDownloadDevice, scriptLog> _command;
        private readonly IMonacoContextFactory _contextFactory;
        private readonly IG2SEgm _egm;
        private readonly IScriptRepository _repository;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetScriptLog" /> class.
        ///     Creates a new instance of the GetScriptLog handler
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="command">Script log command builder.</param>
        /// <param name="contextFactory">DB context factory</param>
        /// <param name="repository">Script log repository</param>
        public GetScriptLog(
            IG2SEgm egm,
            ICommandBuilder<IDownloadDevice, scriptLog> command,
            IMonacoContextFactory contextFactory,
            IScriptRepository repository)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _command = command ?? throw new ArgumentNullException(nameof(command));
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<download, getScriptLog> command)
        {
            return await Sanction.OwnerAndGuests<IDownloadDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<download, getScriptLog> command)
        {
            var dl = _egm.GetDevice<IDownloadDevice>(command.IClass.deviceId);
            if (dl == null)
            {
                return;
            }

            var response = command.GenerateResponse<scriptLogList>();

            using (var context = _contextFactory.Create())
            {
                var logEntries = _repository.GetAll(context);

                response.Command.scriptLog = logEntries
                    .AsEnumerable()
                    .TakeLogs(command.Command.lastSequence, command.Command.totalEntries)
                    .Select(a => ConvertToScriptLog(a, dl))
                    .ToArray();
            }

            await Task.CompletedTask;
        }

        private scriptLog ConvertToScriptLog(Script sle, IDownloadDevice dl)
        {
            var log = new scriptLog { scriptId = sle.ScriptId };

            _command.Build(dl, log);

            return log;
        }
    }
}