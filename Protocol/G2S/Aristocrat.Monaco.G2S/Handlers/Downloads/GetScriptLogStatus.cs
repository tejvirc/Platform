namespace Aristocrat.Monaco.G2S.Handlers.Downloads
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Common.PackageManager;

    /// <summary>
    ///     Handles the v21.getScriptLogStatus G2S message
    /// </summary>
    public class GetScriptLogStatus : ICommandHandler<download, getScriptLogStatus>
    {
        private readonly IG2SEgm _egm;
        private readonly IPackageManager _packageManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetScriptLogStatus" /> class.
        ///     Creates a new instance of the GetScriptLogStatus handler
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="packageManager">Package Manager</param>
        public GetScriptLogStatus(IG2SEgm egm, IPackageManager packageManager)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _packageManager = packageManager ?? throw new ArgumentNullException(nameof(packageManager));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<download, getScriptLogStatus> command)
        {
            return await Sanction.OwnerAndGuests<IDownloadDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<download, getScriptLogStatus> command)
        {
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                var dl = _egm.GetDevice<IDownloadDevice>(command.IClass.deviceId);
                if (dl == null)
                {
                    return;
                }

                var response = command.GenerateResponse<scriptLogStatus>();
                response.Command.lastSequence = _packageManager.GetScriptLastSequence();
                response.Command.totalEntries = _packageManager.ScriptCount;
            }

            await Task.CompletedTask;
        }
    }
}