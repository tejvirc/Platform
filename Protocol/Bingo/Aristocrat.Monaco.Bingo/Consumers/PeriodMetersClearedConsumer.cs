namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Application.Contracts;
    using Common;
    using Kernel;
    using Services.Reporting;

    /// <summary>
    ///     Handles the <see cref="PeriodMetersClearedEvent" /> event.
    /// </summary>
    public class PeriodMetersClearedConsumer : Consumes<PeriodMetersClearedEvent>
    {
        private readonly IReportEventQueueService _bingoServerEventReportingService;

        public PeriodMetersClearedConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            IReportEventQueueService reportingService)
            : base(eventBus, consumerContext)
        {
            _bingoServerEventReportingService = reportingService ?? throw new ArgumentNullException(nameof(reportingService));
        }

        public override void Consume(PeriodMetersClearedEvent _)
        {
            _bingoServerEventReportingService.AddNewEventToQueue(ReportableEvent.ResetPeriod);
        }
    }
}