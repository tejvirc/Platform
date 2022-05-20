namespace Aristocrat.Monaco.G2S.Handlers.Downloads
{
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Services;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    ///     Handles the v21.getPackageLogStatus G2S message
    /// </summary>
    public class GetPackageLogStatus : ICommandHandler<download, getPackageLogStatus>
    {
        private readonly IG2SEgm _egm;
        private readonly IPackageLog _packageLogs;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetPackageLogStatus" /> class.
        ///     Creates a new instance of the GetPackageLogStatus handler
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="packageLog">Package Manager</param>
        public GetPackageLogStatus(IG2SEgm egm, IPackageLog packageLog)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _packageLogs = packageLog ?? throw new ArgumentNullException(nameof(packageLog));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<download, getPackageLogStatus> command)
        {
            return await Sanction.OwnerAndGuests<IDownloadDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<download, getPackageLogStatus> command)
        {
            var response = command.GenerateResponse<packageLogStatus>();

            response.Command.totalEntries = _packageLogs.Entries;
            if (response.Command.totalEntries != 0)
            {
                response.Command.lastSequence = _packageLogs.LastSequence;
            }

            await Task.CompletedTask;
        }
    }
}