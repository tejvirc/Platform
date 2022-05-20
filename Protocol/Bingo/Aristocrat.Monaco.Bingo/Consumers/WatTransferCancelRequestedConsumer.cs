namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Accounting.Contracts.Wat;
    using Common;
    using Kernel;
    using Services.Reporting;

    /// <summary>
    ///     Handles the <see cref="WatTransferCancelRequestedEvent" /> event.
    /// </summary>
    public class WatTransferCancelRequestedConsumer : Consumes<WatTransferCancelRequestedEvent>
    {
        private readonly IReportEventQueueService _bingoEventQueue;

        public WatTransferCancelRequestedConsumer(
            IEventBus eventBus,
            ISharedConsumer sharedConsumer,
            IReportEventQueueService bingoEventQueue)
            : base(eventBus, sharedConsumer)
        {
            _bingoEventQueue = bingoEventQueue ?? throw new ArgumentNullException(nameof(bingoEventQueue));
        }

        public override void Consume(WatTransferCancelRequestedEvent theEvent)
        {
            var transaction = theEvent.Transaction;
            if (transaction.Status == WatStatus.CancelReceived)
            {
                _bingoEventQueue.AddNewEventToQueue(ReportableEvent.TransferOutTimeout);
            }
        }
    }
}