namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Accounting.Contracts;
    using Accounting.Contracts.TransferOut;
    using Application.Contracts.Extensions;
    using Common;
    using Kernel;
    using Services.Reporting;

    /// <summary>
    ///     Handles the <see cref="VoucherIssuedEvent" /> event.
    ///     This event is sent when a voucher has finished printing.
    /// </summary>
    public class VoucherIssuedConsumer : Consumes<VoucherIssuedEvent>
    {
        private readonly IReportTransactionQueueService _bingoTransactionReportHandler;
        private readonly IReportEventQueueService _bingoEventQueue;

        public VoucherIssuedConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            IReportTransactionQueueService bingoTransactionReportHandler,
            IReportEventQueueService bingoEventQueue)
            : base(eventBus, consumerContext)
        {
            _bingoTransactionReportHandler =
                bingoTransactionReportHandler ??
                throw new ArgumentNullException(nameof(bingoTransactionReportHandler));
            _bingoEventQueue = bingoEventQueue ?? throw new ArgumentNullException(nameof(bingoEventQueue));
        }

        public override void Consume(VoucherIssuedEvent theEvent)
        {
            var transaction = theEvent.Transaction;

            if (!transaction.VoucherPrinted)
            {
                _bingoEventQueue.AddNewEventToQueue(ReportableEvent.VoucherIssueTimeout);
                return;
            }

            if (transaction.TransactionAmount == 0)
            {
                return;
            }

            var amountInCents = transaction.TransactionAmount.MillicentsToCents();

            switch (transaction.Reason)
            {
                case TransferOutReason.CashOut:
                    switch (transaction.TypeOfAccount)
                    {
                        case AccountType.Cashable:
                            _bingoTransactionReportHandler.AddNewTransactionToQueue(
                                Common.TransactionType.CashOut,
                                amountInCents);
                            break;
                        case AccountType.Promo:
                            _bingoTransactionReportHandler.AddNewTransactionToQueue(
                                Common.TransactionType.CashPromoTicketOut,
                                amountInCents);
                            break;
                        case AccountType.NonCash:
                            _bingoTransactionReportHandler.AddNewTransactionToQueue(
                                Common.TransactionType.NonTransferablePromoTicketOut,
                                amountInCents);
                            break;
                    }

                    break;
                case TransferOutReason.LargeWin:
                    _bingoTransactionReportHandler.AddNewTransactionToQueue(
                        Common.TransactionType.CashOutJackpot,
                        amountInCents);
                    _bingoEventQueue.AddNewEventToQueue(ReportableEvent.CashoutJackpot);
                    break;

                case TransferOutReason.BonusPay:
                    _bingoTransactionReportHandler.AddNewTransactionToQueue(
                        Common.TransactionType.CashoutBonus,
                        amountInCents);
                    _bingoEventQueue.AddNewEventToQueue(ReportableEvent.CashoutBonus);
                    break;
            }

            _bingoEventQueue.AddNewEventToQueue(ReportableEvent.TicketOut);
        }
    }
}