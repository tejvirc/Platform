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
    using Data.Model;

    /// <summary>
    ///     Handles the v21.authorizeScript G2S message
    /// </summary>
    [ProhibitWhenDisabled]
    public class AuthorizeScript : ICommandHandler<download, authorizeScript>
    {
        private readonly ICommandBuilder<IDownloadDevice, scriptStatus> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;
        private readonly IPackageManager _packageManager;
        private readonly IScriptManager _scriptManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AuthorizeScript" /> class.
        ///     Creates a new instance of the AuthorizeScript handler
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="packageManager">Package Manager</param>
        /// <param name="scriptManager">Script Manager.</param>
        /// <param name="eventLift">Event lift.</param>
        /// <param name="commandBuilder">Script status command builder.</param>
        public AuthorizeScript(
            IG2SEgm egm,
            IPackageManager packageManager,
            IScriptManager scriptManager,
            IEventLift eventLift,
            ICommandBuilder<IDownloadDevice, scriptStatus> commandBuilder)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _packageManager = packageManager ?? throw new ArgumentNullException(nameof(packageManager));
            _scriptManager = scriptManager ?? throw new ArgumentNullException(nameof(scriptManager));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<download, authorizeScript> command)
        {
            return await Sanction.OwnerAndGuests<IDownloadDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<download, authorizeScript> command)
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

                if (script.State == ScriptState.InProgress)
                {
                    command.Error.Code = ErrorCode.G2S_DLX007;
                    return;
                }

                if (script.State == ScriptState.PendingDateTime || script.State == ScriptState.PendingDisable
                    || script.State == ScriptState.PendingAuthorization ||
                    script.State == ScriptState.PendingPackage)
                {
                    if (script.AuthorizeItems != null && script.AuthorizeItems.Count > 0)
                    {
                        foreach (var au in script.AuthorizeItems)
                        {
                            if (command.HostId == au.HostId)
                            {
                                au.AuthorizeStatus = command.Command.authorized
                                    ? AuthorizationState.Authorized
                                    : AuthorizationState.Pending;

                                if (command.Command.authorized)
                                {
                                    script.AuthorizeDateTime = DateTime.UtcNow;
                                    _eventLift.Report(
                                        dl,
                                        EventCode.G2S_DLE202);
                                }
                                else
                                {
                                    _eventLift.Report(
                                        dl,
                                        EventCode.G2S_DLE203);
                                }
                            }
                        }
                    }
                }
                else
                {
                    command.Error.Code = ErrorCode.G2S_DLX007;
                    return;
                }

                _packageManager.UpdateScript(script);

                _scriptManager.AuthorizeScript(script);

                var response = command.GenerateResponse<scriptStatus>();
                response.Command.scriptId = script.ScriptId;

                await _commandBuilder.Build(dl, response.Command);
            }

            await Task.CompletedTask;
        }
    }
}