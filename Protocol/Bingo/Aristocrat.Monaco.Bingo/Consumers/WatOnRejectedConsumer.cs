namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Accounting.Contracts;
    using Common;
    using Kernel;
    using Services.Reporting;

    /// <summary>
    ///     Handles the <see cref="WatOnRejectedEvent" /> event.
    /// </summary>
    public class WatOnRejectedConsumer : Consumes<WatOnRejectedEvent>
    {
        private readonly IReportEventQueueService _bingoEventQueue;

        public WatOnRejectedConsumer(
            IEventBus eventBus,
            ISharedConsumer sharedConsumer,
            IReportEventQueueService bingoEventQueue)
            : base(eventBus, sharedConsumer)
        {
            _bingoEventQueue = bingoEventQueue ?? throw new ArgumentNullException(nameof(bingoEventQueue));
        }

        public override void Consume(WatOnRejectedEvent theEvent)
        {
            _bingoEventQueue.AddNewEventToQueue(ReportableEvent.TransferInTimeout);
        }
    }
}