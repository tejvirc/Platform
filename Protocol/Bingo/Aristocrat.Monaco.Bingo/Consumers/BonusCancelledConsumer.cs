namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Common;
    using Gaming.Contracts.Bonus;
    using Kernel;
    using Services.Reporting;

    /// <summary>
    ///     Handles the <see cref="BonusCancelledEvent" /> event.
    /// </summary>
    public class BonusCancelledConsumer : Consumes<BonusCancelledEvent>
    {
        private readonly IReportEventQueueService _bingoEventQueue;

        public BonusCancelledConsumer(
            IEventBus eventBus,
            ISharedConsumer sharedConsumer,
            IReportEventQueueService bingoEventQueue)
            : base(eventBus, sharedConsumer)
        {
            _bingoEventQueue = bingoEventQueue ?? throw new ArgumentNullException(nameof(bingoEventQueue));
        }

        public override void Consume(BonusCancelledEvent theEvent)
        {
            if (theEvent.Transaction.Mode == BonusMode.GameWin)
            {
                return;
            }

            _bingoEventQueue.AddNewEventToQueue(ReportableEvent.BonusWinRefused);
        }
    }
}