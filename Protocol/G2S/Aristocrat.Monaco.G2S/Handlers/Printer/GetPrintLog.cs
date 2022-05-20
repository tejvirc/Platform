namespace Aristocrat.Monaco.G2S.Handlers.Printer
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
    ///     Implementation of <see cref="getPrintLog" /> for the <see cref="printer" /> class
    /// </summary>
    public class GetPrintLog : ICommandHandler<printer, getPrintLog>
    {
        private readonly IG2SEgm _egm;
        private readonly IPrintLog _printLog;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetPrintLog" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance.</param>
        /// <param name="printLog">An <see cref="IPrintLog" /> instance.</param>
        public GetPrintLog(IG2SEgm egm, IPrintLog printLog)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _printLog = printLog ?? throw new ArgumentNullException(nameof(printLog));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<printer, getPrintLog> command)
        {
            return await Sanction.OwnerAndGuests<IPrinterDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<printer, getPrintLog> command)
        {
            var response = command.GenerateResponse<printLogList>();

            var transactions = _printLog.GetLogs();

            response.Command.printLog = transactions
                .TakeLogs(command.Command.lastSequence, command.Command.totalEntries)
                .Select(transaction => transaction.ToPrintLog()).ToArray();

            await Task.CompletedTask;
        }
    }
}