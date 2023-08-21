namespace Aristocrat.Monaco.G2S.Handlers.Downloads
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Common.PackageManager;
    using Common.PackageManager.Storage;

    /// <summary>
    ///     Handles the v21.cancelScript G2S message
    /// </summary>
    public class CancelScript : ICommandHandler<download, cancelScript>
    {
        private readonly ICommandBuilder<IDownloadDevice, scriptStatus> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly IPackageManager _packageManager;
        private readonly IScriptManager _scriptManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CancelScript" /> class.
        ///     Creates a new instance of the CancelScript handler
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="packageManager">Package Manager</param>
        /// <param name="scriptManager">Script Manager.</param>
        /// <param name="commandBuilder">Script status command builder.</param>
        public CancelScript(
            IG2SEgm egm,
            IPackageManager packageManager,
            IScriptManager scriptManager,
            ICommandBuilder<IDownloadDevice, scriptStatus> commandBuilder)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _packageManager = packageManager ?? throw new ArgumentNullException(nameof(packageManager));
            _scriptManager = scriptManager ?? throw new ArgumentNullException(nameof(scriptManager));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<download, cancelScript> command)
        {
            return await Sanction.OnlyOwner<IDownloadDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<download, cancelScript> command)
        {
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                var dl = _egm.GetDevice<IDownloadDevice>(command.IClass.deviceId);
                if (dl == null)
                {
                    return;
                }

                var script = _packageManager.GetScript(command.Command.scriptId);

                if (script == null)
                {
                    command.Error.Code = ErrorCode.G2S_DLX005;
                    return;
                }

                if (script.State == ScriptState.InProgress || script.State == ScriptState.Completed ||
                    script.State == ScriptState.Canceled || script.State == ScriptState.Error)
                {
                    command.Error.Code = ErrorCode.G2S_DLX007;
                    return;
                }

                _scriptManager.CancelScript(script);

                var response = command.GenerateResponse<scriptStatus>();
                response.Command.scriptId = script.ScriptId;
                await _commandBuilder.Build(dl, response.Command);
            }

            await Task.CompletedTask;
        }
    }
}