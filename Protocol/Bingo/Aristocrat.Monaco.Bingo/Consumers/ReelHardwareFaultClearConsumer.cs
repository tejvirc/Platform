namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using System.Linq;
    using Common;
    using Hardware.Contracts.Reel;
    using Kernel;
    using Services.Reporting;
    using HardwareReelFaultClearEvent = Hardware.Contracts.Reel.Events.HardwareReelFaultClearEvent;

    /// <summary>
    ///     Handles the <see cref="Hardware.Contracts.Reel.Events.HardwareReelFaultClearEvent" /> event for reels.
    /// </summary>
    public class ReelHardwareFaultClearConsumer : Consumes<HardwareReelFaultClearEvent>
    {
        private readonly IReportEventQueueService _bingoServerEventReportingService;

        public ReelHardwareFaultClearConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            IReportEventQueueService reportingService)
            : base(eventBus, consumerContext)
        {
            _bingoServerEventReportingService = reportingService ?? throw new ArgumentNullException(nameof(reportingService));
        }

        /// <summary>
        ///     Consumes a HardwareReelFaultClearEvent and sends a ReelErrorClear event to the server if there are no remaining reel faults.
        /// </summary>
        public override void Consume(HardwareReelFaultClearEvent _)
        {
            var reelController = ServiceManager.GetInstance().TryGetService<IReelController>();

            if (reelController == null)
            {
                return;
            }

            if (reelController.ReelControllerFaults.Equals(ReelControllerFaults.None) && 
                reelController.Faults.Values.All(x => x == ReelFaults.None || x.HasFlag(ReelFaults.Disconnected)))
            {
                _bingoServerEventReportingService.AddNewEventToQueue(ReportableEvent.ReelErrorClear);
            }
        }
    }
}