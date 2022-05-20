namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Accounting.Contracts;
    using Application.Contracts.Extensions;
    using Common;
    using Kernel;
    using Services.Reporting;

    /// <summary>
    ///     Handles the <see cref="CurrencyInCompletedEvent" /> event.
    ///     This event is sent by the CurrencyInProvider when a bill
    ///     has been stacked and the bill value and count have been metered.
    /// </summary>
    public class CurrencyInCompletedConsumer : Consumes<CurrencyInCompletedEvent>
    {
        private readonly IReportTransactionQueueService _bingoServerTransactionReportHandler;
        private readonly IReportEventQueueService _bingoEventQueue;

        public CurrencyInCompletedConsumer(
            IEventBus eventBus,
            ISharedConsumer sharedConsumer,
            IReportTransactionQueueService handler,
            IReportEventQueueService bingoEventQueue)
            : base(eventBus, sharedConsumer)
        {
            _bingoServerTransactionReportHandler = handler ?? throw new ArgumentNullException(nameof(handler));
            _bingoEventQueue = bingoEventQueue ?? throw new ArgumentNullException(nameof(bingoEventQueue));
        }

        public override void Consume(CurrencyInCompletedEvent theEvent)
        {
            if (theEvent.Amount == 0 || theEvent.Note is null)
            {
                return;
            }

            _bingoServerTransactionReportHandler
                .AddNewTransactionToQueue(Common.TransactionType.CashIn, theEvent.Amount.MillicentsToCents());
            _bingoEventQueue.AddNewEventToQueue(ReportableEvent.CashIn);
        }
    }
}
