namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Common;
    using Hardware.Contracts.Reel;
    using Hardware.Contracts.Reel.Events;
    using Kernel;
    using Services.Reporting;

    /// <summary>
    ///     Handles the reel controller <see cref="HardwareFaultEvent" /> event.
    ///     The event covers several possible reel controller faults.
    /// </summary>
    public class HardwareFaultConsumer : Consumes<HardwareFaultEvent>
    {
        private readonly IReportEventQueueService _bingoEventQueue;

        public HardwareFaultConsumer(IEventBus eventBus,
            ISharedConsumer consumerContext,
            IReportEventQueueService bingoEventQueue)
            : base(eventBus, consumerContext)
        {
            _bingoEventQueue = bingoEventQueue ?? throw new ArgumentNullException(nameof(bingoEventQueue));
        }

        public override void Consume(HardwareFaultEvent theEvent)
        {
            switch (theEvent.Fault)
            {
                case ReelControllerFaults.CommunicationError:
                    _bingoEventQueue.AddNewEventToQueue(ReportableEvent.ReelHome);
                    return;
                case ReelControllerFaults.HardwareError:
                    _bingoEventQueue.AddNewEventToQueue(ReportableEvent.ReelWatchdog);
                    return;
            }
        }
    }
}