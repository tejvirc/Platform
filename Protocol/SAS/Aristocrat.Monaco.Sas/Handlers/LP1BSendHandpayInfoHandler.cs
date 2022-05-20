namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Application.Contracts.Extensions;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Kernel;

    /// <summary>
    ///     The handler for LP 1B Send Handpay info
    /// </summary>
    public class LP1BSendHandpayInfoHandler : ISasLongPollHandler<LongPollHandpayDataResponse, LongPollHandpayData>
    {
        private readonly ISasHandPayCommittedHandler _sasHandPayCommittedHandler;
        private readonly ITransactionHistory _transactionHistory;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IBank _bank;

        /// <summary>
        ///     Creates an instance of the LP1BSendHandpayInfoHandler class
        /// </summary>
        /// <param name="sasHandPayCommittedHandler">HandPay Committed handler to notify implied ack after query is successful</param>
        /// <param name="transactionHistory">The transaction history provider</param>
        /// <param name="propertiesManager">The properties manager</param>
        /// <param name="bank">The bank</param>
        public LP1BSendHandpayInfoHandler(
            ISasHandPayCommittedHandler sasHandPayCommittedHandler,
            ITransactionHistory transactionHistory,
            IPropertiesManager propertiesManager,
            IBank bank)
        {
            _sasHandPayCommittedHandler = sasHandPayCommittedHandler ??
                                          throw new ArgumentNullException(nameof(ISasHandPayCommittedHandler));
            _transactionHistory = transactionHistory ?? throw new ArgumentNullException(nameof(transactionHistory));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
        }

        /// <inheritdoc />
        public List<LongPoll> Commands => new List<LongPoll>
        {
            LongPoll.SendHandpayInformation
        };

        /// <inheritdoc />
        public LongPollHandpayDataResponse Handle(LongPollHandpayData data)
        {
            // if no transaction found response will be all zeros as is the response for 1B for no transactions.
            var response = _sasHandPayCommittedHandler.GetNextUnreadHandpayTransaction(data.ClientNumber) ?? new LongPollHandpayDataResponse();

            if (response.HasProgressiveWin)
            {
                response.Amount = response.Amount.MillicentsToCents();
                response.SessionGamePayAmount =
                    response.SessionGamePayAmount.MillicentsToCents();
                response.SessionGameWinAmount =
                    response.SessionGameWinAmount.MillicentsToCents();
            }
            else
            {
                // We have a non progressive win so report in the accounting denom
                response.Amount = response.Amount.MillicentsToAccountCredits(data.AccountingDenom);
                response.SessionGamePayAmount =
                    response.SessionGamePayAmount.MillicentsToAccountCredits(data.AccountingDenom);
                response.SessionGameWinAmount =
                    response.SessionGameWinAmount.MillicentsToAccountCredits(data.AccountingDenom);
            }

            // Get the current state since this may not be while we are in a handpay
            var transaction = _transactionHistory.RecallTransactions<HandpayTransaction>()
                .FirstOrDefault(x => x.TransactionId == response.TransactionId);
            response.ResetId = transaction?.GetResetId(_propertiesManager, _bank) ??
                               ResetId.OnlyStandardHandpayResetIsAvailable;

            // This is odd but the spec is very clear that this is always in the accounting denom
            response.PartialPayAmount = response.PartialPayAmount.MillicentsToAccountCredits(data.AccountingDenom);
            return response;
        }
    }
}
