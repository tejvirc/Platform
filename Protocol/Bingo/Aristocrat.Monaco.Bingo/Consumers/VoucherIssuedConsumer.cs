namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Accounting.Contracts;
    using Accounting.Contracts.TransferOut;
    using Application.Contracts.Extensions;
    using Common;
    using Common.Storage.Model;
    using Kernel;
    using Protocol.Common.Storage.Entity;
    using Services.Reporting;

    /// <summary>
    ///     Handles the <see cref="VoucherIssuedEvent" /> event.
    ///     This event is sent when a voucher has finished printing.
    /// </summary>
    public class VoucherIssuedConsumer : Consumes<VoucherIssuedEvent>
    {
        private readonly IReportTransactionQueueService _bingoTransactionReportHandler;
        private readonly IReportEventQueueService _bingoEventQueue;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IPropertiesManager _propertiesManager;

        public VoucherIssuedConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            IReportTransactionQueueService bingoTransactionReportHandler,
            IReportEventQueueService bingoEventQueue,
            IUnitOfWorkFactory unitOfWorkFactory,
            IPropertiesManager propertiesManager)
            : base(eventBus, consumerContext)
        {
            _bingoTransactionReportHandler =
                bingoTransactionReportHandler ??
                throw new ArgumentNullException(nameof(bingoTransactionReportHandler));
            _bingoEventQueue = bingoEventQueue ?? throw new ArgumentNullException(nameof(bingoEventQueue));
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
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
            var gameConfiguration = _unitOfWorkFactory.GetSelectedGameConfiguration(_propertiesManager);

            switch (transaction.Reason)
            {
                case TransferOutReason.CashOut:
                    HandleCashout(transaction, amountInCents, gameConfiguration);
                    break;
                case TransferOutReason.LargeWin:
                    _bingoTransactionReportHandler.AddNewTransactionToQueue(
                        Common.TransactionType.CashOutJackpot,
                        amountInCents,
                        (uint)(gameConfiguration?.GameTitleId ?? 0),
                        (int)(gameConfiguration?.Denomination ?? 0));
                    _bingoEventQueue.AddNewEventToQueue(ReportableEvent.CashoutJackpot);
                    break;
                case TransferOutReason.BonusPay:
                    _bingoTransactionReportHandler.AddNewTransactionToQueue(
                        Common.TransactionType.CashoutBonus,
                        amountInCents,
                        (uint)(gameConfiguration?.GameTitleId ?? 0),
                        (int)(gameConfiguration?.Denomination ?? 0));
                    _bingoEventQueue.AddNewEventToQueue(ReportableEvent.CashoutBonus);
                    break;
            }

            _bingoEventQueue.AddNewEventToQueue(ReportableEvent.TicketOut);
        }

        private void HandleCashout(
            VoucherOutTransaction transaction,
            long amountInCents,
            BingoGameConfiguration gameConfiguration)
        {
            switch (transaction.TypeOfAccount)
            {
                case AccountType.Cashable:
                    _bingoTransactionReportHandler.AddNewTransactionToQueue(
                        Common.TransactionType.CashOut,
                        amountInCents,
                        (uint)(gameConfiguration?.GameTitleId ?? 0),
                        (int)(gameConfiguration?.Denomination ?? 0));
                    break;
                case AccountType.Promo:
                    _bingoTransactionReportHandler.AddNewTransactionToQueue(
                        Common.TransactionType.CashPromoTicketOut,
                        amountInCents,
                        (uint)(gameConfiguration?.GameTitleId ?? 0),
                        (int)(gameConfiguration?.Denomination ?? 0));
                    break;
                case AccountType.NonCash:
                    _bingoTransactionReportHandler.AddNewTransactionToQueue(
                        Common.TransactionType.NonTransferablePromoTicketOut,
                        amountInCents,
                        (uint)(gameConfiguration?.GameTitleId ?? 0),
                        (int)(gameConfiguration?.Denomination ?? 0));
                    break;
            }
        }
    }
}