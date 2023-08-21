namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Kernel;

    /// <inheritdoc />
    public class LP94RemoteHandPayResetHandler :
        ISasLongPollHandler<LongPollReadSingleValueResponse<HandPayResetCode>, LongPollData>
    {
        private readonly ITransactionHistory _transactionHistory;
        private readonly IEventBus _eventBus;
        private readonly IPropertiesManager _properties;

        /// <inheritdoc />
        public List<LongPoll> Commands => new List<LongPoll>
        {
            LongPoll.RemoteHandpayReset
        };

        /// <summary>
        ///     Creates the LP94RemoteHandPayResetHandler
        /// </summary>
        /// <param name="transactionHistory">The transaction history</param>
        /// <param name="eventBus">The event bus</param>
        /// <param name="properties">The event bus</param>
        public LP94RemoteHandPayResetHandler(ITransactionHistory transactionHistory, IEventBus eventBus, IPropertiesManager properties)
        {
            _transactionHistory = transactionHistory ?? throw new ArgumentNullException(nameof(transactionHistory));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        /// <inheritdoc />
        public LongPollReadSingleValueResponse<HandPayResetCode> Handle(LongPollData data)
        {
            var transaction = _transactionHistory.RecallTransactions<HandpayTransaction>()
                .OrderBy(x => x.TransactionDateTime)
                .FirstOrDefault(x => x.State == HandpayState.Pending);
            if (transaction == null)
            {
                return new LongPollReadSingleValueResponse<HandPayResetCode>(HandPayResetCode.NotInHandpay);
            }

            if (!_properties.GetValue(AccountingConstants.RemoteHandpayResetAllowed, true) || _properties.GetValue(AccountingConstants.MenuSelectionHandpayInProgress, false))
            {
                return new LongPollReadSingleValueResponse<HandPayResetCode>(HandPayResetCode.UnableToResetHandpay);
            }

            var keyOffType = KeyOffType.RemoteHandpay;
            if (transaction.KeyOffType == KeyOffType.LocalCredit)
            {
                keyOffType = KeyOffType.RemoteCredit;
            }

            // We never authorize non cash credits to hand paid for SAS
            _eventBus.Publish(
                new RemoteKeyOffEvent(
                    keyOffType,
                    transaction.CashableAmount,
                    transaction.PromoAmount,
                    0));
            return new LongPollReadSingleValueResponse<HandPayResetCode>(HandPayResetCode.HandpayWasReset);
        }
    }
}