namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Application.Contracts.OperatorMenu;
    using Common;
    using Kernel;
    using Services.Reporting;

    /// <summary>
    ///     Handles the <see cref="OperatorMenuEnteredEvent" /> event.
    /// </summary>
    public class OperatorMenuEnteredConsumer : Consumes<OperatorMenuEnteredEvent>
    {
        private readonly IReportEventQueueService _bingoServerEventReportingService;

        public OperatorMenuEnteredConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            IReportEventQueueService reportingService)
            : base(eventBus, consumerContext)
        {
            _bingoServerEventReportingService = reportingService ?? throw new ArgumentNullException(nameof(reportingService));
        }

        public override void Consume(OperatorMenuEnteredEvent _)
        {
            _bingoServerEventReportingService.AddNewEventToQueue(ReportableEvent.Operator);
            _bingoServerEventReportingService.AddNewEventToQueue(ReportableEvent.SetUpModeEntered);
        }
    }
}