namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Common;
    using Hardware.Contracts.Reel;
    using Kernel;
    using Services.Reporting;

    /// <summary>
    ///     Handles the <see cref="HardwareReelFaultEvent" /> event.
    ///     The event covers several possible reel faults.
    /// </summary>
    public class HardwareReelFaultConsumer : Consumes<HardwareReelFaultEvent>
    {
        private readonly IReportEventQueueService _bingoEventQueue;

        public HardwareReelFaultConsumer(IEventBus eventBus,
            ISharedConsumer consumerContext,
            IReportEventQueueService bingoEventQueue)
            : base(eventBus, consumerContext)
        {
            _bingoEventQueue = bingoEventQueue ?? throw new ArgumentNullException(nameof(bingoEventQueue));
        }

        public override void Consume(HardwareReelFaultEvent theEvent)
        {
            switch (theEvent.Fault)
            {
                case ReelFaults.LowVoltage:
                    _bingoEventQueue.AddNewEventToQueue(ReportableEvent.ReelBrownOut);
                    break;
                case ReelFaults.ReelTamper:
                    _bingoEventQueue.AddNewEventToQueue(ReportableEvent.ReelFeedback);
                    break;
                case ReelFaults.ReelStall:
                    switch (theEvent.ReelId)
                    {
                        case 0: // unknown reel
                            _bingoEventQueue.AddNewEventToQueue(ReportableEvent.ReelError14);
                            break;
                        case 1:
                            _bingoEventQueue.AddNewEventToQueue(ReportableEvent.ReelError8);
                            break;
                        case 2:
                            _bingoEventQueue.AddNewEventToQueue(ReportableEvent.ReelError9);
                            break;
                        case 3:
                            _bingoEventQueue.AddNewEventToQueue(ReportableEvent.ReelError10);
                            break;
                        case 4:
                            _bingoEventQueue.AddNewEventToQueue(ReportableEvent.ReelError11);
                            break;
                        case 5:
                            _bingoEventQueue.AddNewEventToQueue(ReportableEvent.ReelError12);
                            break;
                        case 6:
                            _bingoEventQueue.AddNewEventToQueue(ReportableEvent.ReelError13);
                            break;
                    }

                    break;
            }
        }
    }
}