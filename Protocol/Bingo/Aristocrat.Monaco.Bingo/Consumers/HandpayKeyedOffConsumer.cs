namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Accounting.Contracts.Handpay;
    using Application.Contracts.Extensions;
    using Common;
    using Kernel;
    using Services.Reporting;

    /// <summary>
    ///     Handles the <see cref="HandpayKeyedOffEvent" /> event.
    ///     This event is sent when a voucher has finished printing.
    /// </summary>
    public class HandpayKeyedOffConsumer : Consumes<HandpayKeyedOffEvent>
    {
        private readonly IReportTransactionQueueService _bingoTransactionReportHandler;
        private readonly IReportEventQueueService _bingoEventQueue;

        public HandpayKeyedOffConsumer(
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

        public override void Consume(HandpayKeyedOffEvent theEvent)
        {
            var transaction = theEvent.Transaction;
            if (transaction.TransactionAmount == 0)
            {
                return;
            }

            var amountInCents = transaction.TransactionAmount.MillicentsToCents();

            if (transaction.HandpayType == HandpayType.CancelCredit)
            {
                _bingoTransactionReportHandler.AddNewTransactionToQueue(
                    TransactionType.CancelledCredits,
                    amountInCents);
                _bingoEventQueue.AddNewEventToQueue(ReportableEvent.CancelCredits);
            }

            _bingoTransactionReportHandler.AddNewTransactionToQueue(
                TransactionType.HandPayKeyOff,
                amountInCents);
            _bingoEventQueue.AddNewEventToQueue(ReportableEvent.HandpayKeyOff);
        }
    }
}