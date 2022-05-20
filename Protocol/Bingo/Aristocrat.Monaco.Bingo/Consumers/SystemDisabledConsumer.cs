namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Common;
    using Kernel;
    using Services.Reporting;

    public class SystemDisabledConsumer : Consumes<SystemDisabledEvent>
    {
        private readonly IReportEventQueueService _bingoServerEventReportingService;

        public SystemDisabledConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            IReportEventQueueService reportingService)
            : base(eventBus, consumerContext)
        {
            _bingoServerEventReportingService = reportingService ?? throw new ArgumentNullException(nameof(reportingService));
        }

        public override void Consume(SystemDisabledEvent _)
        {
            _bingoServerEventReportingService.AddNewEventToQueue(ReportableEvent.Disabled);
        }
    }
}