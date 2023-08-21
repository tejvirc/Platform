namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Common;
    using Gaming.Contracts;
    using Kernel;
    using Services.Reporting;

    public class CallAttendantButtonOnConsumer : Consumes<CallAttendantButtonOnEvent>
    {
        private readonly IReportEventQueueService _bingoServerEventReportingService;

        public CallAttendantButtonOnConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            IReportEventQueueService reportingService)
            : base(eventBus, consumerContext)
        {
            _bingoServerEventReportingService = reportingService ?? throw new ArgumentNullException(nameof(reportingService));
        }

        public override void Consume(CallAttendantButtonOnEvent _)
        {
            _bingoServerEventReportingService.AddNewEventToQueue(ReportableEvent.CallAttendantButtonActivated);
        }
    }
}