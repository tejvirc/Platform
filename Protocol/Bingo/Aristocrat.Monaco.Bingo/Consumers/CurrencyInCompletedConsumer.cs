﻿namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Accounting.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Common;
    using Kernel;
    using Protocol.Common.Storage.Entity;
    using Services.Reporting;

    /// <summary>
    ///     Handles the <see cref="CurrencyInCompletedEvent" /> event.
    ///     This event is sent by the CurrencyInProvider when a bill
    ///     has been stacked and the bill value and count have been metered.
    /// </summary>
    public class CurrencyInCompletedConsumer : Consumes<CurrencyInCompletedEvent>
    {
        private readonly IReportTransactionQueueService _bingoServerTransactionReportHandler;
        private readonly IReportEventQueueService _bingoEventQueue;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IGameProvider _gameProvider;

        public CurrencyInCompletedConsumer(
            IEventBus eventBus,
            ISharedConsumer sharedConsumer,
            IReportTransactionQueueService handler,
            IReportEventQueueService bingoEventQueue,
            IUnitOfWorkFactory unitOfWorkFactory,
            IGameProvider gameProvider)
            : base(eventBus, sharedConsumer)
        {
            _bingoServerTransactionReportHandler = handler ?? throw new ArgumentNullException(nameof(handler));
            _bingoEventQueue = bingoEventQueue ?? throw new ArgumentNullException(nameof(bingoEventQueue));
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
        }

        public override void Consume(CurrencyInCompletedEvent theEvent)
        {
            if (theEvent.Amount == 0 || theEvent.Note is null)
            {
                return;
            }

            var gameConfiguration = _unitOfWorkFactory.GetSelectedGameConfiguration(_gameProvider);
            _bingoServerTransactionReportHandler
                .AddNewTransactionToQueue(
                    Common.TransactionType.CashIn,
                    theEvent.Amount.MillicentsToCents(),
                    (uint)(gameConfiguration?.GameTitleId ?? 0),
                    (int)(gameConfiguration?.Denomination.MillicentsToCents() ?? 0));
            _bingoEventQueue.AddNewEventToQueue(ReportableEvent.CashIn);
        }
    }
}
