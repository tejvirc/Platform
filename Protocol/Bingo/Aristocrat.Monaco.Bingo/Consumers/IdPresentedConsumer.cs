namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Common;
    using Hardware.Contracts.IdReader;
    using Kernel;
    using Services.Reporting;

    /// <summary>
    ///     Handles the <see cref="IdPresentedEvent" /> event.
    ///     This event is sent by the card reader when a card is inserted
    /// </summary>
    public class IdPresentedConsumer : Consumes<IdPresentedEvent>
    {
        private readonly IReportEventQueueService _bingoServerEventReportingService;

        public IdPresentedConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            IReportEventQueueService reportingService)
            : base(eventBus, consumerContext)
        {
            _bingoServerEventReportingService = reportingService ?? throw new ArgumentNullException(nameof(reportingService));
        }

        public override void Consume(IdPresentedEvent theEvent)
        {
            _bingoServerEventReportingService.AddNewEventToQueue(ReportableEvent.CardInserted);
        }
    }
}