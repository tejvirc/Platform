namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Common;
    using Hardware.Contracts.IdReader;
    using Kernel;
    using Services.Reporting;

    /// <summary>
    ///     Handles the <see cref="IdClearedEvent" /> event.
    ///     This event is sent by the card reader when a card is removed
    /// </summary>
    public class IdClearedConsumer : Consumes<IdClearedEvent>
    {
        private readonly IReportEventQueueService _bingoServerEventReportingService;

        public IdClearedConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            IReportEventQueueService reportingService)
            : base(eventBus, consumerContext)
        {
            _bingoServerEventReportingService = reportingService ?? throw new ArgumentNullException(nameof(reportingService));
        }

        public override void Consume(IdClearedEvent theEvent)
        {
            _bingoServerEventReportingService.AddNewEventToQueue(ReportableEvent.CardRemoved);
        }
    }
}