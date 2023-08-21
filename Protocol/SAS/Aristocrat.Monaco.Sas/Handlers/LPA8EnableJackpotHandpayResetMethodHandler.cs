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

    /// <summary>
    ///     Handler for sending enabled features from games or the EGM broadly.
    /// </summary>
    public class LPa8EnableJackpotHandpayResetMethodHandler : ISasLongPollHandler<EnableJackpotHandpayResetMethodResponse, EnableJackpotHandpayResetMethodData>
    {
        private readonly ITransactionHistory _transactionHistory;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IEventBus _eventBus;
        private readonly IBank _bank;

        /// <summary>
        ///     Creates a new instance of the LPA8EnableJackpotHandpayResetMethodHandler class.
        /// </summary>
        public LPa8EnableJackpotHandpayResetMethodHandler(ITransactionHistory transactionHistory, IPropertiesManager propertiesManager, IEventBus eventBus, IBank bank)
        {
            _transactionHistory = transactionHistory ?? throw new ArgumentNullException(nameof(ITransactionHistory));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(IPropertiesManager));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
        }

        /// <inheritdoc/>
        public List<LongPoll> Commands { get; } = new List<LongPoll> { LongPoll.EnableJackpotHandpayResetMethod };

        /// <inheritdoc/>
        /// <param name="data">
        ///     Single value uint used to identify a target game. SAS requires it as an argument,
        ///     but it is currently not in use.
        /// </param>
        public EnableJackpotHandpayResetMethodResponse Handle(EnableJackpotHandpayResetMethodData data)
        {
            var transaction = _transactionHistory.RecallTransactions<HandpayTransaction>()
                .OrderBy(x => x.TransactionDateTime)
                .FirstOrDefault(x => x.State == HandpayState.Pending || x.State == HandpayState.Requested);

            if (transaction == null)
            {
                return new EnableJackpotHandpayResetMethodResponse(AckCode.NotCurrentlyInAHandpayCondition);
            }

            if (transaction.GetResetId(_propertiesManager, _bank) == ResetId.OnlyStandardHandpayResetIsAvailable ||
                _propertiesManager.GetValue(AccountingConstants.MenuSelectionHandpayInProgress, false))
            {
                return new EnableJackpotHandpayResetMethodResponse(AckCode.UnableToEnableResetMethod);
            }

            switch (data.Method)
            {
                case ResetMethod.StandardHandpay:
                    _eventBus.Publish(
                        new RemoteKeyOffEvent(
                            KeyOffType.LocalHandpay,
                            transaction.CashableAmount,
                            transaction.PromoAmount,
                            transaction.NonCashAmount));
                    return new EnableJackpotHandpayResetMethodResponse(AckCode.ResetMethodEnabled);
                case ResetMethod.ResetToTheCreditMeter:
                    _eventBus.Publish(
                        new RemoteKeyOffEvent(
                            KeyOffType.LocalCredit,
                            transaction.CashableAmount,
                            transaction.PromoAmount,
                            transaction.NonCashAmount));
                    return new EnableJackpotHandpayResetMethodResponse(AckCode.ResetMethodEnabled);
                default:
                    return new EnableJackpotHandpayResetMethodResponse(AckCode.UnableToEnableResetMethod);
            }
        }
    }
}
