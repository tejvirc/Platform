namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Accounting.Contracts.Handpay;
    using Application.Contracts.Extensions;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Common;
    using Kernel;
    using Protocol.Common.Storage.Entity;
    using Services.Reporting;

    /// <summary>
    ///     Handles the <see cref="HandpayKeyedOffEvent" /> event.
    ///     This event is sent when a voucher has finished printing.
    /// </summary>
    public class HandpayKeyedOffConsumer : Consumes<HandpayKeyedOffEvent>
    {
        private readonly IReportTransactionQueueService _bingoTransactionReportHandler;
        private readonly IReportEventQueueService _bingoEventQueue;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IGameProvider _gameProvider;

        public HandpayKeyedOffConsumer(
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

        public override void Consume(HandpayKeyedOffEvent theEvent)
        {
            var transaction = theEvent.Transaction;
            if (transaction.TransactionAmount == 0)
            {
                return;
            }

            var amountInCents = transaction.TransactionAmount.MillicentsToCents();
            var gameConfiguration = _unitOfWorkFactory.GetSelectedGameConfiguration(_gameProvider);

            switch (transaction.HandpayType)
            {
                case HandpayType.CancelCredit:
                    _bingoTransactionReportHandler.AddNewTransactionToQueue(
                        TransactionType.CancelledCredits,
                        amountInCents,
                        (uint)(gameConfiguration?.GameTitleId ?? 0),
                        (int)(gameConfiguration?.Denomination.MillicentsToCents() ?? 0),
                        transaction.Barcode);
                    _bingoEventQueue.AddNewEventToQueue(ReportableEvent.HandpayKeyedOffCancelCredits);
                    break;
                case HandpayType.GameWin when !transaction.IsCreditType():
                    _bingoTransactionReportHandler.AddNewTransactionToQueue(
                        TransactionType.Jackpot,
                        amountInCents,
                        (uint)(gameConfiguration?.GameTitleId ?? 0),
                        (int)(gameConfiguration?.Denomination.MillicentsToCents() ?? 0),
                        transaction.Barcode);
                    _bingoEventQueue.AddNewEventToQueue(ReportableEvent.HandpayKeyedOffJackpot);
                    break;
                case HandpayType.BonusPay:
                    _bingoTransactionReportHandler.AddNewTransactionToQueue(
                        TransactionType.ExternalBonusLargeWin,
                        amountInCents,
                        (uint)(gameConfiguration?.GameTitleId ?? 0),
                        (int)(gameConfiguration?.Denomination.MillicentsToCents() ?? 0),
                        transaction.Barcode);
                    // *NOTE* BonusLargeWinReported already added to event queue by BonusAwardedConsumer
                    if (!transaction.IsCreditType())
                    {
                        _bingoTransactionReportHandler.AddNewTransactionToQueue(
                            TransactionType.Jackpot,
                            amountInCents,
                            (uint)(gameConfiguration?.GameTitleId ?? 0),
                            (int)(gameConfiguration?.Denomination.MillicentsToCents() ?? 0),
                            transaction.Barcode);
                        _bingoEventQueue.AddNewEventToQueue(ReportableEvent.HandpayKeyedOffCashoutExternalBonus);
                    }

                    break;
            }
        }
    }
}