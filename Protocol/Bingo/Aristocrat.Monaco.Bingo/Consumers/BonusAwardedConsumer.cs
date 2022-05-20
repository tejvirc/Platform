namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Common;
    using Gaming.Contracts.Bonus;
    using Kernel;
    using Services.Reporting;

    /// <summary>
    ///     Handles the <see cref="BonusAwardedEvent" /> event.
    /// </summary>
    public class BonusAwardedConsumer : Consumes<BonusAwardedEvent>
    {
        private readonly IReportEventQueueService _bingoEventQueue;

        public BonusAwardedConsumer(
            IEventBus eventBus,
            ISharedConsumer sharedConsumer,
            IReportEventQueueService bingoEventQueue)
            : base(eventBus, sharedConsumer)
        {
            _bingoEventQueue = bingoEventQueue ?? throw new ArgumentNullException(nameof(bingoEventQueue));
        }

        public override void Consume(BonusAwardedEvent theEvent)
        {
            var transaction = theEvent.Transaction;

            if (transaction.Mode == BonusMode.GameWin)
            {
                return;
            }

            // large win if it has to be hand paid
            _bingoEventQueue.AddNewEventToQueue(
                transaction.PayMethod == PayMethod.Handpay
                    ? ReportableEvent.BonusLargeWinAwarded
                    : ReportableEvent.BonusWinAwarded);
        }
    }
}