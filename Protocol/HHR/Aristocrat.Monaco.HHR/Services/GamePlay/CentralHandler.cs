namespace Aristocrat.Monaco.Hhr.Services.GamePlay
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Commands;
    using Events;
    using Gaming.Contracts;
    using Gaming.Contracts.Central;
    using Kernel;
    using log4net;
    using Storage.Helpers;

    /// <summary>
    ///     A <see cref="ICentralHandler" /> implementation
    /// </summary>
    public class CentralHandler : ICentralHandler, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IBank _bank;
        private readonly ICommandHandlerFactory _commandFactory;
        private readonly ICentralProvider _centralProvider;
        private readonly IEventBus _eventBus;
        private readonly IGameProvider _gameProvider;

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CentralHandler" /> class.
        /// </summary>
        public CentralHandler(
            ICentralProvider centralProvider,
            ICommandHandlerFactory commandFactory,
            IBank bank,
            IEventBus eventBus,
            IGameProvider gameProvider,
            IGamePlayEntityHelper gamePlayEntityHelper,
            ITransactionHistory transactionHistory)
        {
            _centralProvider = centralProvider ?? throw new ArgumentNullException(nameof(centralProvider));
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));

            if (transactionHistory == null) throw new ArgumentException(nameof(transactionHistory));

            _centralProvider.Register(this, ProtocolNames.HHR);

            var history = _centralProvider.Transactions
                .Where(t => t.OutcomeState == OutcomeState.Committed)
                .OrderBy(h => h.TransactionId).ToList();
            foreach (var log in history)
            {
                _centralProvider.AcknowledgeOutcome(log.TransactionId);
            }

            if (gamePlayEntityHelper.PrizeCalculationError)
            {
                _eventBus.Publish(new PrizeCalculationErrorEvent());
            }

            _eventBus.Subscribe<OutcomeReceivedEvent>(
                this,
                evt =>
                {
                    // The transaction must be acknowledged, but this doesn't mean anything to the central server
                    _centralProvider.AcknowledgeOutcome(evt.Transaction.TransactionId);
                });

            _eventBus.Subscribe<OutcomeFailedEvent>(
                this,
                evt =>
                {
                    _cancellationTokenSource.Cancel();
                    _cancellationTokenSource.Dispose();
                    _cancellationTokenSource = new CancellationTokenSource();
                });
        }

        /// <inheritdoc />
        public async Task RequestOutcomes(CentralTransaction transaction, bool isRecovering = false)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            Logger.Info($"Requesting outcomes for {transaction}");

            var currentGame = _gameProvider.GetGame(transaction.GameId);

            await _commandFactory.Execute(
                new RequestPlay(
                    (int)_bank.QueryBalance(AccountType.Cashable).MillicentsToCents(),
                    (int)_bank.QueryBalance(AccountType.Promo).MillicentsToCents(),
                    Convert.ToInt32(transaction.WagerCategory),
                    (int)(transaction.WagerAmount / transaction.Denomination),
                    (int)transaction.Denomination.MillicentsToCents(),
                    Convert.ToInt32(currentGame.ReferenceId),
                    (int)transaction.TransactionId,
                    transaction.OutcomesRequested,
                    isRecovering,
                    _cancellationTokenSource.Token));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Releases allocated resources.
        /// </summary>
        /// <param name="disposing">
        ///     true to release both managed and unmanaged resources; false to release only unmanaged resources.
        /// </param>
        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _centralProvider.Clear(ProtocolNames.HHR);

                _eventBus.UnsubscribeAll(this);

                _cancellationTokenSource.Dispose();
            }

            _disposed = true;
        }
    }
}