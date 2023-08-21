namespace Aristocrat.Monaco.G2S.Handlers.Voucher
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Handles the v21.getVoucherLogStatus G2S message
    /// </summary>
    public class GetVoucherLogStatus : ICommandHandler<voucher, getVoucherLogStatus>
    {
        private readonly IG2SEgm _egm;
        private readonly ITransactionHistory _transactionHistory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetVoucherLogStatus" /> class.
        ///     Creates a new instance of the GetDownloadStatus handler
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="transactionHistory">Transaction history</param>
        public GetVoucherLogStatus(IG2SEgm egm, ITransactionHistory transactionHistory)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _transactionHistory = transactionHistory ?? throw new ArgumentNullException(nameof(transactionHistory));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<voucher, getVoucherLogStatus> command)
        {
            return await Sanction.OwnerAndGuests<IVoucherDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<voucher, getVoucherLogStatus> command)
        {
            var transactions = _transactionHistory.RecallTransactions<VoucherOutTransaction>();
            var inTransactions = _transactionHistory.RecallTransactions<VoucherInTransaction>();
            var response = command.GenerateResponse<voucherLogStatus>();

            response.Command.totalEntries = transactions.Count + inTransactions.Count;
            if (transactions.Count != 0)
            {
                response.Command.lastSequence = transactions.Max(x => x.LogSequence);
            }

            if (inTransactions.Count != 0)
            {
                var last = inTransactions.Max(x => x.LogSequence);

                if (last > response.Command.lastSequence)
                {
                    response.Command.lastSequence = last;
                }
            }

            await Task.CompletedTask;
        }
    }
}