namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Application.Contracts.OperatorMenu;
    using Common;
    using Kernel;
    using Services.Reporting;

    /// <summary>
    ///     Handles the <see cref="OperatorMenuExitedEvent" /> event.
    /// </summary>
    public class OperatorMenuExitedConsumer : Consumes<OperatorMenuExitedEvent>
    {
        private readonly IReportEventQueueService _bingoServerEventReportingService;

        public OperatorMenuExitedConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            IReportEventQueueService reportingService)
            : base(eventBus, consumerContext)
        {
            _bingoServerEventReportingService = reportingService ?? throw new ArgumentNullException(nameof(reportingService));
        }

        public override void Consume(OperatorMenuExitedEvent _)
        {
            _bingoServerEventReportingService.AddNewEventToQueue(ReportableEvent.SetUpModeExited);
        }
    }
}