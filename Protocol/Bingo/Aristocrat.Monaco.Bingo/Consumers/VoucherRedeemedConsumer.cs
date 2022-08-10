namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Accounting.Contracts;
    using Application.Contracts.Extensions;
    using Common;
    using Kernel;
    using Protocol.Common.Storage.Entity;
    using Services.Reporting;

    /// <summary>
    ///     Handles the <see cref="VoucherRedeemedEvent" /> event.
    ///     This event is sent when a voucher has been accepted.
    /// </summary>
    public class VoucherRedeemedConsumer : Consumes<VoucherRedeemedEvent>
    {
        private readonly IReportTransactionQueueService _bingoTransactionReportHandler;
        private readonly IReportEventQueueService _bingoEventQueue;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IPropertiesManager _propertiesManager;

        public VoucherRedeemedConsumer(
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

        public override void Consume(VoucherRedeemedEvent theEvent)
        {
            var transaction = theEvent.Transaction;
            if (transaction.Amount == 0)
            {
                return;
            }

            var amountInCents = transaction.Amount.MillicentsToCents();
            var gameConfiguration = _unitOfWorkFactory.GetSelectedGameConfiguration(_propertiesManager);

            var transactionType = Common.TransactionType.TicketIn;
            switch (transaction.TypeOfAccount)
            {
                case AccountType.Promo:
                    transactionType = Common.TransactionType.CashPromoTicketIn;
                    break;

                case AccountType.NonCash:
                    transactionType = Common.TransactionType.NonTransferablePromoTicketIn;
                    break;
            }

            _bingoTransactionReportHandler.AddNewTransactionToQueue(
                transactionType,
                amountInCents,
                (uint)(gameConfiguration?.GameTitleId ?? 0),
                (int)(gameConfiguration?.Denomination.MillicentsToCents() ?? 0));
            _bingoEventQueue.AddNewEventToQueue(ReportableEvent.TicketIn);
        }
    }
}