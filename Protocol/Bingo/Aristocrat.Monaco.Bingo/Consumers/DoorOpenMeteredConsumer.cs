namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Hardware.Contracts.Door;
    using Kernel;
    using Services.Reporting;

    public class DoorOpenMeteredConsumer : Consumes<DoorOpenMeteredEvent>
    {
        private readonly IReportEventQueueService _bingoServerEventReportingService;

        public DoorOpenMeteredConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            IReportEventQueueService reportingService)
            : base(eventBus, consumerContext)
        {
            _bingoServerEventReportingService = reportingService ?? throw new ArgumentNullException(nameof(reportingService));
        }

        public override void Consume(DoorOpenMeteredEvent theEvent)
        {
            if ((!theEvent.WhilePoweredDown || !theEvent.IsRecovery) &&
                BingoDoorToEvent.DoorEventMap.TryGetValue(
                (DoorLogicalId)theEvent.LogicalId,
                out var openInfo))
            {
                _bingoServerEventReportingService.AddNewEventToQueue(openInfo.Opened);
            }
        }
    }
}