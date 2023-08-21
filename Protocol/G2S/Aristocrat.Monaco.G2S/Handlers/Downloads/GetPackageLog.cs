namespace Aristocrat.Monaco.G2S.Handlers.Downloads
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Services;

    /// <summary>
    ///     Handles the v21.getPackageLog G2S message
    /// </summary>
    public class GetPackageLog : ICommandHandler<download, getPackageLog>
    {
        private readonly IG2SEgm _egm;
        private readonly IPackageLog _packageLog;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetPackageLog" /> class.
        ///     Creates a new instance of the GetPackageLog handler
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="packageLog">Package logs.</param>
        public GetPackageLog(
            IG2SEgm egm,
            IPackageLog packageLog)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _packageLog = packageLog ?? throw new ArgumentNullException(nameof(packageLog));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<download, getPackageLog> command)
        {
            return await Sanction.OwnerAndGuests<IDownloadDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<download, getPackageLog> command)
        {
            var response = command.GenerateResponse<packageLogList>();

            var transactions = _packageLog.GetLogs();

            response.Command.packageLog = transactions
                .TakeLogs(command.Command.lastSequence, command.Command.totalEntries)
                .Select(transaction => transaction.ToPackageLog()).ToArray();

            await Task.CompletedTask;
        }
    }
}