namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Accounting.Contracts;
    using Accounting.Contracts.TransferOut;
    using Application.Contracts.Extensions;
    using Aristocrat.Monaco.Gaming.Contracts;
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
        private readonly IGameProvider _gameProvider;

        public VoucherIssuedConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            IReportTransactionQueueService bingoTransactionReportHandler,
            IReportEventQueueService bingoEventQueue,
            IUnitOfWorkFactory unitOfWorkFactory,
            IGameProvider gameProvider)
            : base(eventBus, consumerContext)
        {
            _bingoTransactionReportHandler =
                bingoTransactionReportHandler ??
                throw new ArgumentNullException(nameof(bingoTransactionReportHandler));
            _bingoEventQueue = bingoEventQueue ?? throw new ArgumentNullException(nameof(bingoEventQueue));
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
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
            var gameConfiguration = _unitOfWorkFactory.GetSelectedGameConfiguration(_gameProvider);

            switch (transaction.Reason)
            {
                case TransferOutReason.CashOut:
                    HandleCashout(transaction, amountInCents, gameConfiguration);
                    break;
                case TransferOutReason.LargeWin:
                    _bingoTransactionReportHandler.AddNewTransactionToQueue(
                        Common.TransactionType.Jackpot,
                        amountInCents,
                        (uint)(gameConfiguration?.GameTitleId ?? 0),
                        (int)(gameConfiguration?.Denomination.MillicentsToCents() ?? 0),
                        transaction.Barcode);
                    _bingoEventQueue.AddNewEventToQueue(ReportableEvent.VoucherIssuedJackpot);
                    break;
                case TransferOutReason.BonusPay:
                    _bingoTransactionReportHandler.AddNewTransactionToQueue(
                        Common.TransactionType.CashoutExternalBonus,
                        amountInCents,
                        (uint)(gameConfiguration?.GameTitleId ?? 0),
                        (int)(gameConfiguration?.Denomination.MillicentsToCents() ?? 0),
                        transaction.Barcode);
                    break;
                case TransferOutReason.CashWin:
                    _bingoTransactionReportHandler.AddNewTransactionToQueue(
                        Common.TransactionType.CashOut,
                        amountInCents,
                        (uint)(gameConfiguration?.GameTitleId ?? 0),
                        (int)(gameConfiguration?.Denomination.MillicentsToCents() ?? 0),
                        transaction.Barcode);
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
                        (int)(gameConfiguration?.Denomination.MillicentsToCents() ?? 0),
                        transaction.Barcode);
                    break;
                case AccountType.Promo:
                    _bingoTransactionReportHandler.AddNewTransactionToQueue(
                        Common.TransactionType.CashPromoTicketOut,
                        amountInCents,
                        (uint)(gameConfiguration?.GameTitleId ?? 0),
                        (int)(gameConfiguration?.Denomination.MillicentsToCents() ?? 0),
                        transaction.Barcode);
                    break;
                case AccountType.NonCash:
                    _bingoTransactionReportHandler.AddNewTransactionToQueue(
                        Common.TransactionType.TransferablePromoTicketOut,
                        amountInCents,
                        (uint)(gameConfiguration?.GameTitleId ?? 0),
                        (int)(gameConfiguration?.Denomination.MillicentsToCents() ?? 0),
                        transaction.Barcode);
                    break;
            }
        }
    }
}