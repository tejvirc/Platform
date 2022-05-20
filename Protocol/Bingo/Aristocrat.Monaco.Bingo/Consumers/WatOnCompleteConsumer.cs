﻿namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Accounting.Contracts;
    using Accounting.Contracts.Wat;
    using Application.Contracts.Extensions;
    using Common;
    using Kernel;
    using Services.Reporting;

    /// <summary>
    ///     Handles the <see cref="WatOnCompleteEvent" /> event.
    ///     This event is sent when a transfer on completes.
    /// </summary>
    public class WatOnCompleteConsumer : Consumes<WatOnCompleteEvent>
    {
        private readonly IReportTransactionQueueService _bingoTransactionReportHandler;
        private readonly IReportEventQueueService _bingoEventQueue;

        public WatOnCompleteConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            IReportTransactionQueueService bingoTransactionReportHandler,
            IReportEventQueueService bingoEventQueue)
            : base(eventBus, consumerContext)
        {
            _bingoTransactionReportHandler =
                bingoTransactionReportHandler ??
                throw new ArgumentNullException(nameof(bingoTransactionReportHandler));
            _bingoEventQueue = bingoEventQueue ?? throw new ArgumentNullException(nameof(bingoEventQueue));
        }

        public override void Consume(WatOnCompleteEvent theEvent)
        {
            var transaction = theEvent.Transaction;
            if (transaction.Status == WatStatus.Rejected)
            {
                _bingoEventQueue.AddNewEventToQueue(ReportableEvent.TransferInRefusedByEgm);
                return;
            }

            if (transaction.EgmException != 0)
            {
                _bingoEventQueue.AddNewEventToQueue(ReportableEvent.TransferInFailed);
                return;
            }

            if (transaction.Status != WatStatus.Complete
                && transaction.Status != WatStatus.Committed)
            {
                return;
            }

            // report partial or full transfers
            _bingoEventQueue.AddNewEventToQueue(
                transaction.AllowReducedAmounts
                    ? ReportableEvent.PartialTransferInComplete
                    : ReportableEvent.TransferInComplete);

            if (transaction.TransferredCashableAmount != 0)
            {
                _bingoTransactionReportHandler.AddNewTransactionToQueue(
                    Common.TransactionType.TransferIn,
                    transaction.TransferredCashableAmount.MillicentsToCents());
            }

            if (transaction.TransferredPromoAmount != 0)
            {
                _bingoTransactionReportHandler.AddNewTransactionToQueue(
                    Common.TransactionType.CashPromoTransferIn,
                    transaction.TransferredPromoAmount.MillicentsToCents());
            }

            if (transaction.TransferredNonCashAmount != 0)
            {
                _bingoTransactionReportHandler.AddNewTransactionToQueue(
                    Common.TransactionType.NonTransferablePromoTransferIn,
                    transaction.TransferredNonCashAmount.MillicentsToCents());
            }
        }
    }
}