namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Accounting.Contracts.Handpay;
    using Application.Contracts.Extensions;
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
        private readonly IPropertiesManager _propertiesManager;

        public HandpayKeyedOffConsumer(
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

        public override void Consume(HandpayKeyedOffEvent theEvent)
        {
            var transaction = theEvent.Transaction;
            if (transaction.TransactionAmount == 0)
            {
                return;
            }

            var amountInCents = transaction.TransactionAmount.MillicentsToCents();
            var gameConfiguration = _unitOfWorkFactory.GetSelectedGameConfiguration(_propertiesManager);

            switch (transaction.HandpayType)
            {
                case HandpayType.CancelCredit:
                    _bingoTransactionReportHandler.AddNewTransactionToQueue(
                        TransactionType.CancelledCredits,
                        amountInCents,
                        (uint)(gameConfiguration?.GameTitleId ?? 0),
                        (int)(gameConfiguration?.Denomination.MillicentsToCents() ?? 0),
                        transaction.Barcode);
                    _bingoEventQueue.AddNewEventToQueue(ReportableEvent.CancelCredits);
                    break;
                case HandpayType.GameWin when !transaction.IsCreditType():
                    _bingoTransactionReportHandler.AddNewTransactionToQueue(
                        TransactionType.CashOutJackpot,
                        amountInCents,
                        (uint)(gameConfiguration?.GameTitleId ?? 0),
                        (int)(gameConfiguration?.Denomination.MillicentsToCents() ?? 0),
                        transaction.Barcode);
                    _bingoEventQueue.AddNewEventToQueue(ReportableEvent.CashoutJackpot);
                    break;
                case HandpayType.BonusPay:
                    _bingoTransactionReportHandler.AddNewTransactionToQueue(
                        TransactionType.ExternalBonusLargeWin,
                        amountInCents,
                        (uint)(gameConfiguration?.GameTitleId ?? 0),
                        (int)(gameConfiguration?.Denomination.MillicentsToCents() ?? 0),
                        transaction.Barcode);
                    if (!transaction.IsCreditType())
                    {
                        _bingoTransactionReportHandler.AddNewTransactionToQueue(
                            TransactionType.CashOutJackpot,
                            amountInCents,
                            (uint)(gameConfiguration?.GameTitleId ?? 0),
                            (int)(gameConfiguration?.Denomination.MillicentsToCents() ?? 0),
                            transaction.Barcode);
                        _bingoEventQueue.AddNewEventToQueue(ReportableEvent.CashoutExternalBonus);
                    }

                    break;
            }

            _bingoTransactionReportHandler.AddNewTransactionToQueue(
                TransactionType.HandPayKeyOff,
                amountInCents,
                (uint)(gameConfiguration?.GameTitleId ?? 0),
                (int)(gameConfiguration?.Denomination.MillicentsToCents() ?? 0),
                transaction.Barcode);
            _bingoEventQueue.AddNewEventToQueue(ReportableEvent.HandpayKeyOff);
        }
    }
}