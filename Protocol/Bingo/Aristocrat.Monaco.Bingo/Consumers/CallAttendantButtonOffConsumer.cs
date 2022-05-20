namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Common;
    using Gaming.Contracts;
    using Kernel;
    using Services.Reporting;

    public class CallAttendantButtonOffConsumer : Consumes<CallAttendantButtonOffEvent>
    {
        private readonly IReportEventQueueService _bingoServerEventReportingService;

        public CallAttendantButtonOffConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            IReportEventQueueService reportingService)
            : base(eventBus, consumerContext)
        {
            _bingoServerEventReportingService = reportingService ?? throw new ArgumentNullException(nameof(reportingService));
        }

        public override void Consume(CallAttendantButtonOffEvent _)
        {
            _bingoServerEventReportingService.AddNewEventToQueue(ReportableEvent.CallAttendantButtonDeactivated);
        }
    }
}