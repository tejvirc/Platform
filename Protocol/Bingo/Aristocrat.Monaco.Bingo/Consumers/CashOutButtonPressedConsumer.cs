namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Common;
    using Gaming.Contracts;
    using Kernel;
    using Services.Reporting;

    /// <summary>
    ///     Handles the <see cref="CashOutButtonPressedEvent" /> event.
    /// </summary>
    public class CashOutButtonPressedConsumer : Consumes<CashOutButtonPressedEvent>
    {
        private readonly IReportEventQueueService _bingoServerEventReportingService;

        public CashOutButtonPressedConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            IReportEventQueueService reportingService)
            : base(eventBus, consumerContext)
        {
            _bingoServerEventReportingService =
                reportingService ?? throw new ArgumentNullException(nameof(reportingService));
        }

        public override void Consume(CashOutButtonPressedEvent buttonPressed)
        {
            _bingoServerEventReportingService.AddNewEventToQueue(
                ReportableEvent.CashOutButtonPressed);
        }
    }
}