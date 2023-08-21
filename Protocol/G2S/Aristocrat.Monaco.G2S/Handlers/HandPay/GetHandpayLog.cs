namespace Aristocrat.Monaco.G2S.Handlers.Handpay
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Services;

    public class GetHandpayLog : ICommandHandler<handpay, getHandpayLog>
    {
        private readonly IG2SEgm _egm;
        private readonly IHandpayProperties _properties;
        private readonly ITransactionReferenceProvider _referenceProvider;
        private readonly ITransactionHistory _transactionHistory;

        public GetHandpayLog(
            IG2SEgm egm,
            ITransactionHistory transactionHistory,
            IHandpayProperties properties,
            ITransactionReferenceProvider referenceProvider)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _transactionHistory = transactionHistory ?? throw new ArgumentNullException(nameof(transactionHistory));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _referenceProvider = referenceProvider ?? throw new ArgumentNullException(nameof(referenceProvider));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<handpay, getHandpayLog> command)
        {
            return await Sanction.OwnerAndGuests<IHandpayDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<handpay, getHandpayLog> command)
        {
            var response = command.GenerateResponse<handpayLogList>();

            var transactions = _transactionHistory.RecallTransactions<HandpayTransaction>();

            response.Command.handpayLog = transactions
                .TakeTransactions(command.Command.lastSequence, command.Command.totalEntries)
                .Select(transaction => transaction.GetLog(_properties, _referenceProvider))
                .ToArray();

            await Task.CompletedTask;
        }
    }
}