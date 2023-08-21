namespace Aristocrat.Monaco.G2S.Handlers.Downloads
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Common.PackageManager;
    using Common.PackageManager.Storage;

    /// <summary>
    ///     Handles the v21.getScriptList G2S message
    /// </summary>
    public class GetScriptList : ICommandHandler<download, getScriptList>
    {
        private readonly ICommandBuilder<IDownloadDevice, scriptStatus> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly IPackageManager _packageManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetScriptList" /> class.
        ///     Creates a new instance of the GetScriptList handler
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="packageManager">Package Manager</param>
        /// <param name="commandBuilder">Script status command builder.</param>
        public GetScriptList(
            IG2SEgm egm,
            IPackageManager packageManager,
            ICommandBuilder<IDownloadDevice, scriptStatus> commandBuilder)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _packageManager = packageManager ?? throw new ArgumentNullException(nameof(packageManager));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<download, getScriptList> command)
        {
            return await Sanction.OwnerAndGuests<IDownloadDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<download, getScriptList> command)
        {
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                var dl = _egm.GetDevice<IDownloadDevice>(command.IClass.deviceId);
                if (dl == null)
                {
                    return;
                }

                var res = command.GenerateResponse<scriptList>();

                var list = new List<scriptStatus>();

                foreach (var script in _packageManager.ScriptEntityList)
                {
                    if (script.State != ScriptState.Canceled
                        && script.State != ScriptState.Completed
                        && script.State != ScriptState.Error)
                    {
                        var status = new scriptStatus { scriptId = script.ScriptId };
                        await _commandBuilder.Build(dl, status);
                        list.Add(status);
                    }
                }

                res.Command.scriptStatus = list.ToArray();
            }

            await Task.CompletedTask;
        }
    }
}