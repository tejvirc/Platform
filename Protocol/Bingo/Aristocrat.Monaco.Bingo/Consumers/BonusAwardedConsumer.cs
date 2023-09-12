namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Application.Contracts.Extensions;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Common;
    using Gaming.Contracts.Bonus;
    using Kernel;
    using Protocol.Common.Storage.Entity;
    using Services.Reporting;

    /// <summary>
    ///     Handles the <see cref="BonusAwardedEvent" /> event.
    /// </summary>
    public class BonusAwardedConsumer : Consumes<BonusAwardedEvent>
    {
        private readonly IReportEventQueueService _bingoEventQueue;
        private readonly IReportTransactionQueueService _transactionQueue;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IGameProvider _gameProvider;

        public BonusAwardedConsumer(
            IEventBus eventBus,
            ISharedConsumer sharedConsumer,
            IReportEventQueueService bingoEventQueue,
            IReportTransactionQueueService transactionQueue,
            IUnitOfWorkFactory unitOfWorkFactory,
            IGameProvider gameProvider)
            : base(eventBus, sharedConsumer)
        {
            _bingoEventQueue = bingoEventQueue ?? throw new ArgumentNullException(nameof(bingoEventQueue));
            _transactionQueue = transactionQueue ?? throw new ArgumentNullException(nameof(transactionQueue));
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
        }

        public override void Consume(BonusAwardedEvent theEvent)
        {
            var transaction = theEvent.Transaction;

            if (transaction.Mode is BonusMode.GameWin)
            {
                return;
            }

            // large win if it has to be hand paid
            _bingoEventQueue.AddNewEventToQueue(
                transaction.PayMethod == PayMethod.Handpay
                    ? ReportableEvent.BonusLargeWinAwarded
                    : ReportableEvent.BonusWinAwarded);
            if (transaction.PayMethod is PayMethod.Handpay or PayMethod.Voucher)
            {
                return;
            }

            var gameConfiguration = _unitOfWorkFactory.GetSelectedGameConfiguration(_gameProvider);
            _transactionQueue.AddNewTransactionToQueue(
                TransactionType.ExternalBonusWin,
                transaction.PaidAmount.MillicentsToCents(),
                (uint)(gameConfiguration?.GameTitleId ?? 0),
                (int)(gameConfiguration?.Denomination.MillicentsToCents() ?? 0));
        }
    }
}