namespace Aristocrat.Monaco.G2S.Handlers.Printer
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Services;

    /// <summary>
    ///     Implementation of <see cref="getPrintLogStatus" /> for the <see cref="printer" /> class
    /// </summary>
    public class GetPrintLogStatus : ICommandHandler<printer, getPrintLogStatus>
    {
        private readonly IG2SEgm _egm;
        private readonly IPrintLog _printLog;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetPrintLogStatus" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance.</param>
        /// <param name="printLog">An <see cref="IPrintLog" /> instance.</param>
        public GetPrintLogStatus(IG2SEgm egm, IPrintLog printLog)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _printLog = printLog ?? throw new ArgumentNullException(nameof(printLog));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<printer, getPrintLogStatus> command)
        {
            return await Sanction.OwnerAndGuests<IPrinterDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<printer, getPrintLogStatus> command)
        {
            var response = command.GenerateResponse<printLogStatus>();

            response.Command.totalEntries = _printLog.Entries;
            if (response.Command.totalEntries != 0)
            {
                response.Command.lastSequence = _printLog.LastSequence;
            }

            await Task.CompletedTask;
        }
    }
}