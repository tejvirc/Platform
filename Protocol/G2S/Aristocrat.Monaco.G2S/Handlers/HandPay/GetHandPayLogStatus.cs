namespace Aristocrat.Monaco.G2S.Handlers.Handpay
{
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using System.Linq;
    using System.Threading.Tasks;

    public class GetHandpayLogStatus : ICommandHandler<handpay, getHandpayLogStatus>
    {
        private readonly IG2SEgm _egm;
        private readonly ITransactionHistory _transactionHistory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetHandpayProfile" /> class.
        /// </summary>
        /// <param name="egm">The <see cref="IG2SEgm" /> object</param>
        /// <param name="transactionHistory">Transaction history</param>
        public GetHandpayLogStatus(
            IG2SEgm egm,
            ITransactionHistory transactionHistory)
        {
            _egm = egm;
            _transactionHistory = transactionHistory;
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<handpay, getHandpayLogStatus> command)
        {
            return await Sanction.OwnerAndGuests<IHandpayDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<handpay, getHandpayLogStatus> command)
        {
            var transactions = _transactionHistory.RecallTransactions<HandpayTransaction>();
            var response = command.GenerateResponse<handpayLogStatus>();

            response.Command.totalEntries = transactions.Count;
            if (transactions.Count > 0)
            {
                response.Command.lastSequence = transactions.Max(x => x.LogSequence);
            }

            await Task.CompletedTask;
        }
    }
}
