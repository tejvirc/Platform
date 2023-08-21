namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Hardware.Contracts.Door;
    using Kernel;
    using Services.Reporting;

    public class DoorClosedMeteredConsumer : Consumes<DoorClosedMeteredEvent>
    {
        private readonly IReportEventQueueService _bingoServerEventReportingService;

        public DoorClosedMeteredConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            IReportEventQueueService reportingService)
            : base(eventBus, consumerContext)
        {
            _bingoServerEventReportingService = reportingService ?? throw new ArgumentNullException(nameof(reportingService));
        }

        public override void Consume(DoorClosedMeteredEvent theEvent)
        {
            if (BingoDoorToEvent.DoorEventMap.TryGetValue(
                    (DoorLogicalId)theEvent.LogicalId,
                    out var openInfo))
            {
                _bingoServerEventReportingService.AddNewEventToQueue(openInfo.Closed);
            }
        }
    }
}