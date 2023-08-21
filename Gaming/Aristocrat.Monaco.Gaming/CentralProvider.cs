namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Accounting.Contracts;
    using Application.Contracts.Protocol;
    using Common;
    using Contracts;
    using Contracts.Central;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;
    using Runtime;
    using Runtime.Client;

    public class CentralProvider : ICentralProvider, IService, IDisposable
    {
        private const int DeviceId = 1;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly ITransactionHistory _transactionHistory;
        private readonly IGameHistory _gameHistory;
        private readonly IPersistentStorageManager _storage;
        private readonly IRuntime _runtime;
        private readonly IEventBus _bus;
        private readonly IMultiProtocolConfigurationProvider _protocolProvider;
        private readonly IPropertiesManager _propertiesManager;
        private CancellationTokenSource _runtimeCancellation;
        private bool _disposed;

        public CentralProvider(
            ITransactionHistory transactionHistory,
            IGameHistory gameHistory,
            IPersistentStorageManager storage,
            IRuntime runtime,
            IEventBus bus,
            IMultiProtocolConfigurationProvider protocolProvider,
            IPropertiesManager propertiesManager)
        {
            _transactionHistory = transactionHistory ?? throw new ArgumentNullException(nameof(transactionHistory));
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _protocolProvider = protocolProvider ?? throw new ArgumentNullException(nameof(protocolProvider));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
        }

        public ICentralHandler Handler { get; private set; }

        public bool Register(ICentralHandler handler, string name)
        {
            if (!IsHandledBy(name))
            {
                return false;
            }

            Handler = handler;
            return true;
        }

        public bool Clear(string name)
        {
            if (!IsHandledBy(name))
            {
                return false;
            }

            Handler = null;
            return true;
        }

        public IEnumerable<CentralTransaction> Transactions =>
            _transactionHistory.TransactionTypes.Contains(typeof(CentralTransaction)) ?
                _transactionHistory.RecallTransactions<CentralTransaction>() :
                Enumerable.Empty<CentralTransaction>();

        public bool RequestOutcomes(
            int gameId,
            long denomination,
            string wagerCategory,
            string templateId,
            long wager,
            IOutcomeRequest request,
            bool recovering)
        {
            if (Handler == null)
            {
                Logger.Warn("No handler registered");
                return false;
            }

            if (wager == 0 || request.Quantity == 0)
            {
                Logger.Error($"Wager({wager}) and Count({request.Quantity}) must be non-zero");
                return false;
            }

            _runtimeCancellation?.Cancel(false);
            _runtimeCancellation?.Dispose();
            _runtimeCancellation = new CancellationTokenSource();

            CentralTransaction transaction = null;

            if (recovering)
            {
                var current = Transactions.FirstOrDefault(
                    t => t.AssociatedTransactions.Contains(_gameHistory.CurrentLog.TransactionId));
                if (current != null)
                {
                    Logger.Info($"Recovering transaction: {current}");
                    switch (current.OutcomeState)
                    {
                        case OutcomeState.Failed:
                            return false;
                        case OutcomeState.Requested:
                            transaction = current;
                            break;
                        default:
                            NotifyRuntime(current);
                            return current.Exception == OutcomeException.None;
                    }
                }
            }

            if (transaction == null)
            {
                transaction = new CentralTransaction(
                    DeviceId,
                    DateTime.UtcNow,
                    gameId,
                    denomination,
                    wagerCategory,
                    templateId,
                    wager,
                    request.Quantity)
                {
                    Exception = OutcomeException.Pending,
                    AssociatedTransactions = new List<long> { _gameHistory.CurrentLog.TransactionId }
                };

                var lastTransaction = Transactions.OrderByDescending(x => x.TransactionId).FirstOrDefault();
                if (!_propertiesManager.GetValue(GamingConstants.KeepFailedGameOutcomes, true) &&
                    lastTransaction?.OutcomeState == OutcomeState.Failed)
                {
                    transaction.LogSequence = lastTransaction.LogSequence;
                    _transactionHistory.OverwriteTransaction(lastTransaction.TransactionId, transaction);
                }
                else
                {
                    _transactionHistory.AddTransaction(transaction);
                }
            }

            _bus.Publish(new OutcomeRequestedEvent(transaction));

            Logger.Info($"Requesting central outcomes: {transaction}");

            Handler.RequestOutcomes(transaction, recovering)
                .FireAndForget(
                    ex =>
                    {
                        Failed(transaction, OutcomeException.TimedOut);

                        Logger.Error("Failed to retrieve outcomes", ex);
                    });

            return true;
        }

        public void OutcomeResponse(long transactionId, IEnumerable<Outcome> outcomes, OutcomeException exception, IEnumerable<IOutcomeDescription> descriptions)
        {
            // Game Outcomes that have a host retry will be transactionId 0
            if (transactionId == 0)
            {
                transactionId = _transactionHistory.RecallTransactions<CentralTransaction>()
                                    .LastOrDefault(t => t.Exception.Equals(OutcomeException.Pending))?.TransactionId ??
                                0;
            }

            var transaction = _transactionHistory.RecallTransactions<CentralTransaction>()
                .FirstOrDefault(t => t.TransactionId == transactionId);

            if (transaction == null)
            {
                return;
            }

            var outcomeList = outcomes.ToList();

            using (var scope = _storage.ScopedTransaction())
            {
                // The outcome storage is redundant, but the game history and transaction logs may roll at different intervals
                _gameHistory.AppendOutcomes(outcomeList);

                if (exception != OutcomeException.None)
                {
                    Failed(transaction, exception);

                    scope.Complete();

                    return;
                }

                transaction.OutcomeState = OutcomeState.Committed;
                transaction.Exception = OutcomeException.None;
                transaction.Outcomes = outcomeList;
                transaction.Descriptions = descriptions?.ToList() ?? Enumerable.Empty<IOutcomeDescription>();

                _transactionHistory.UpdateTransaction(transaction);

                scope.Complete();
            }

            NotifyRuntime(transaction);

            Logger.Info($"Outcomes received: {transaction}");

            _bus.Publish(new OutcomeReceivedEvent(transaction));
        }

        public void AcknowledgeOutcome(long transactionId)
        {
            var transaction = _transactionHistory.RecallTransactions<CentralTransaction>()
                .FirstOrDefault(t => t.TransactionId == transactionId);

            if (transaction == null || transaction.OutcomeState == OutcomeState.Acknowledged)
            {
                return;
            }

            transaction.OutcomeState = OutcomeState.Acknowledged;
            _transactionHistory.UpdateTransaction(transaction);
            _bus.Publish(new OutcomeCommittedEvent(transaction));
        }

        public void UpdateOutcomeDescription(long transactionId, IEnumerable<IOutcomeDescription> descriptions)
        {
            var transaction = _transactionHistory.RecallTransactions<CentralTransaction>()
                .FirstOrDefault(t => t.TransactionId == transactionId);

            if (transaction == null)
            {
                return;
            }

            transaction.Descriptions = descriptions.ToList();
            _transactionHistory.UpdateTransaction(transaction);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public string Name => typeof(CentralProvider).FullName;

        public ICollection<Type> ServiceTypes => new[] { typeof(ICentralProvider) };

        public void Initialize()
        {
            _bus.Subscribe<GameProcessExitedEvent>(this, _ => _runtimeCancellation?.Cancel(false));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _runtimeCancellation?.Cancel(false);
                DisposeRuntimeCancellation();

                _bus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private void DisposeRuntimeCancellation()
        {
            // ReSharper disable once UseNullPropagation
            if (_runtimeCancellation != null)
            {
                _runtimeCancellation.Dispose();
                _runtimeCancellation = null;
            }
        }

        private void Failed(CentralTransaction transaction, OutcomeException exception)
        {
            transaction.OutcomeState = OutcomeState.Failed;
            transaction.Exception = exception;

            _transactionHistory.UpdateTransaction(transaction);

            NotifyRuntime(exception, Enumerable.Empty<Outcome>());

            Logger.Warn($"Failed to get outcome: {transaction}");

            _bus.Publish(new OutcomeFailedEvent(transaction));
        }

        private void NotifyRuntime(CentralTransaction transaction)
        {
            NotifyRuntime(transaction.Exception, transaction.Outcomes);
        }

        private void NotifyRuntime(OutcomeException exception, IEnumerable<Outcome> outcomes)
        {
            if(!(_runtimeCancellation?.IsCancellationRequested ?? false))
            {
                _runtime.BeginGameRoundResponse(ToResult(), outcomes, _runtimeCancellation);
            }

            DisposeRuntimeCancellation();

            BeginGameRoundResult ToResult()
            {
                switch (exception)
                {
                    case OutcomeException.TimedOut:
                        return BeginGameRoundResult.TimedOut;
                    case OutcomeException.None:
                        return BeginGameRoundResult.Success;
                    default:
                        return BeginGameRoundResult.Failed;
                }
            }
        }

        private bool IsHandledBy(string protocol)
        {
            return _protocolProvider.MultiProtocolConfiguration.ToList()
                .Any(x => x.Protocol == EnumParser.ParseOrThrow<CommsProtocol>(protocol) && x.IsCentralDeterminationHandled);
        }
    }
}