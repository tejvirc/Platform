namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Common;
    using Gaming.Contracts;
    using Kernel;
    using Services.Reporting;

    public class GamePlayDisabledConsumer : Consumes<GamePlayDisabledEvent>
    {
        private readonly IReportEventQueueService _bingoServerEventReportingService;

        public GamePlayDisabledConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            IReportEventQueueService reportingService)
            : base(eventBus, consumerContext)
        {
            _bingoServerEventReportingService = reportingService ?? throw new ArgumentNullException(nameof(reportingService));
        }

        public override void Consume(GamePlayDisabledEvent _)
        {
            _bingoServerEventReportingService.AddNewEventToQueue(ReportableEvent.Disabled);
        }
    }
}