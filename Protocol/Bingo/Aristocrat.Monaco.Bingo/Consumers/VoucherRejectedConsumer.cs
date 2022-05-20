namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Accounting.Contracts;
    using Accounting.Contracts.Vouchers;
    using Common;
    using Kernel;
    using Services.Reporting;

    /// <summary>
    ///     Handles the <see cref="VoucherRejectedEvent" /> event.
    ///     This event is sent when a voucher has been rejected.
    /// </summary>
    public class VoucherRejectedConsumer : Consumes<VoucherRejectedEvent>
    {
        private readonly IReportEventQueueService _bingoEventQueue;

        public VoucherRejectedConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            IReportEventQueueService bingoEventQueue)
            : base(eventBus, consumerContext)
        {
            _bingoEventQueue = bingoEventQueue ?? throw new ArgumentNullException(nameof(bingoEventQueue));
        }

        public override void Consume(VoucherRejectedEvent theEvent)
        {
            var transaction = theEvent.Transaction;
            switch (transaction.Exception)
            {
                case (int)VoucherInExceptionCode.VoucherInLimitExceeded:
                    _bingoEventQueue.AddNewEventToQueue(ReportableEvent.CashInExceedsVoucherInLimit);
                    break;
                case (int)VoucherInExceptionCode.TimedOut:
                    _bingoEventQueue.AddNewEventToQueue(ReportableEvent.VoucherRedeemTimeout);
                    break;
            }
        }
    }
}
