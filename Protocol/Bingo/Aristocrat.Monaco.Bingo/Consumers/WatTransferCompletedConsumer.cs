namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Accounting.Contracts.Wat;
    using Application.Contracts.Extensions;
    using Common;
    using Kernel;
    using Protocol.Common.Storage.Entity;
    using Services.Reporting;

    /// <summary>
    ///     Handles the <see cref="WatTransferCompletedEvent" /> event.
    ///     This event is sent when a transfer off completes.
    /// </summary>
    public class WatTransferCompletedConsumer : Consumes<WatTransferCompletedEvent>
    {
        private readonly IReportTransactionQueueService _bingoTransactionReportHandler;
        private readonly IReportEventQueueService _bingoEventQueue;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IPropertiesManager _propertiesManager;

        public WatTransferCompletedConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            IReportTransactionQueueService bingoTransactionReportHandler,
            IReportEventQueueService bingoEventQueue,
            IUnitOfWorkFactory unitOfWorkFactory,
            IPropertiesManager propertiesManager)
            : base(eventBus, consumerContext)
        {
            _bingoTransactionReportHandler =
                bingoTransactionReportHandler ??
                throw new ArgumentNullException(nameof(bingoTransactionReportHandler));
            _bingoEventQueue = bingoEventQueue ?? throw new ArgumentNullException(nameof(bingoEventQueue));
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
        }

        public override void Consume(WatTransferCompletedEvent theEvent)
        {
            var transaction = theEvent.Transaction;
            if (transaction.Status == WatStatus.Rejected)
            {
                _bingoEventQueue.AddNewEventToQueue(ReportableEvent.TransferOutRefusedByEgm);
                return;
            }

            if (transaction.EgmException != 0)
            {
                _bingoEventQueue.AddNewEventToQueue(ReportableEvent.TransferOutFailed);
                return;
            }

            if (transaction.Status != WatStatus.Complete)
            {
                return;
            }

            // report partial or full transfers
            _bingoEventQueue.AddNewEventToQueue(transaction.AllowReducedAmounts
                    ? ReportableEvent.PartialTransferOutComplete
                    : ReportableEvent.TransferOutComplete);

            var gameConfiguration = _unitOfWorkFactory.GetSelectedGameConfiguration(_propertiesManager);
            if (transaction.TransferredCashableAmount != 0)
            {
                _bingoTransactionReportHandler.AddNewTransactionToQueue(
                    TransactionType.TransferOut,
                    transaction.TransferredCashableAmount.MillicentsToCents(),
                    (uint)(gameConfiguration?.GameTitleId ?? 0),
                    (int)(gameConfiguration?.Denomination.MillicentsToCents() ?? 0));
            }

            if (transaction.TransferredPromoAmount != 0)
            {
                _bingoTransactionReportHandler.AddNewTransactionToQueue(
                    TransactionType.CashPromoTransferOut,
                    transaction.TransferredPromoAmount.MillicentsToCents(),
                    (uint)(gameConfiguration?.GameTitleId ?? 0),
                    (int)(gameConfiguration?.Denomination.MillicentsToCents() ?? 0));
            }

            if (transaction.TransferredNonCashAmount != 0)
            {
                _bingoTransactionReportHandler.AddNewTransactionToQueue(
                    TransactionType.NonTransferablePromoTransferOut,
                    transaction.TransferredNonCashAmount.MillicentsToCents(),
                    (uint)(gameConfiguration?.GameTitleId ?? 0),
                    (int)(gameConfiguration?.Denomination.MillicentsToCents() ?? 0));
            }
        }
    }
}