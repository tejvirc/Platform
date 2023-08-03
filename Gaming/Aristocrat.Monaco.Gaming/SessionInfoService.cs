namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Accounting.Contracts.HandCount;
    using Accounting.Contracts.Handpay;
    using Accounting.Contracts.Wat;
    using Application.Contracts.Extensions;
    using Contracts;
    using Hardware.Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;

    public class SessionInfoService : ISessionInfoService, IService
    {
        private const string StartTransactionIdField = @"StartTransactionId";
        private const string EndEventTypeField = @"EndEventType";
        private const string EndTransactionIdField = @"EndTransactionId";
        private const string SessionStartedField = @"SessionStarted";
        private const string CashedOutDuringSessionField = @"CashedOutDuringSession";
        private const PersistenceLevel Level = PersistenceLevel.Critical;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly object _lock = new object();

        private IPersistentStorageAccessor _accessor;
        private IBank _bank;
        private IEventBus _eventBus;
        private IGamePlayState _gamePlayState;
        private ITransactionHistory _transactionHistory;
        private long _lastTransactionId;
        private long _lastOutAmount;
        private long _startTransactionId;
        private SessionEventType _endEventType;
        private long _endTransactionId;
        private bool _sessionStarted;
        private bool _cashedOutDuringSession;
        private bool _disposed;

        public void Initialize()
        {
            Logger.Debug("SessionInfoService Initializing");
            var blockName = GetType().ToString();
            _accessor = ServiceManager.GetInstance().GetService<IPersistentStorageManager>()
                .GetAccessor(Level, blockName);

            _startTransactionId = (long)(_accessor[StartTransactionIdField] ?? 0);
            _endEventType = (SessionEventType)(_accessor[EndEventTypeField] ?? SessionEventType.None);
            _endTransactionId = (long)(_accessor[EndTransactionIdField] ?? 0);
            _sessionStarted = (bool)(_accessor[SessionStartedField] ?? false);
            _cashedOutDuringSession = (bool)(_accessor[CashedOutDuringSessionField] ?? false);

            _eventBus = ServiceManager.GetInstance().GetService<IEventBus>();

            Task.Run(
                () =>
                {
                    var waiter = new ServiceWaiter(_eventBus);
                    waiter.AddServiceToWaitFor<IBank>();
                    waiter.AddServiceToWaitFor<IGamePlayState>();
                    waiter.AddServiceToWaitFor<ITransactionHistory>();
                    if (waiter.WaitForServices())
                    {
                        _bank = ServiceManager.GetInstance().GetService<IBank>();
                        _gamePlayState = ServiceManager.GetInstance().GetService<IGamePlayState>();
                        _transactionHistory = ServiceManager.GetInstance().GetService<ITransactionHistory>();

                        _eventBus.Subscribe<GameIdleEvent>(this, HandleEvent);

                        Logger.Debug(
                            $"SessionInfoService Initialized with values startTransactionId: {_startTransactionId}, endEventType: {_endEventType}, endTransactionId: {_endTransactionId}, sessionStarted: {_sessionStarted}, cashedOutDuringSession: {_cashedOutDuringSession}");
                    }
                });
        }

        public string Name => typeof(ISessionInfoService).ToString();

        public ICollection<Type> ServiceTypes => new[] { typeof(ISessionInfoService) };

        public void HandleTransaction(ITransaction transaction)
        {
            Logger.Debug($"Handling {transaction.GetType()} Transaction ID: {transaction.TransactionId}");

            switch (transaction)
            {
                case KeyedOffCreditsTransaction tran:
                    HandleCreditOutEvent(
                        SessionEventType.KeyedOff,
                        tran.TransactionId,
                        tran.TransactionAmount);
                    break;
                case KeyedOnCreditsTransaction tran:
                    TraceStartSession(tran.TransactionId);
                    break;
                case VoucherInTransaction _:
                case WatOnTransaction _:
                case BillTransaction _:
                    TraceStartSession(transaction.TransactionId);
                    break;
                case HandpayTransaction handpay:
                    HandleCreditOutEvent(
                        SessionEventType.Handpay,
                        transaction.TransactionId,
                        handpay.TransactionAmount);
                    break;
                case WatTransaction wat:
                    HandleCreditOutEvent(SessionEventType.Wat, transaction.TransactionId, wat.TransactionAmount);
                    break;
                case VoucherOutTransaction voucher:
                    HandleCreditOutEvent(
                        SessionEventType.VoucherOut,
                        transaction.TransactionId,
                        voucher.TransactionAmount);
                    break;
                case HardMeterOutTransaction hardMeterOut:
                    HandleCreditOutEvent(
                        SessionEventType.HardMeterOut,
                        transaction.TransactionId,
                        hardMeterOut.TransactionAmount);
                    break;
            }
        }

        public double GetSessionPaidValue()
        {
            lock (_lock)
            {
                if (!_sessionStarted || _cashedOutDuringSession)
                {
                    var amount = _lastOutAmount;
                    var lastTransactionId = _lastTransactionId;
                    if (amount == 0 || lastTransactionId == 0)
                    {
                        switch (_endEventType)
                        {
                            case SessionEventType.KeyedOff:
                                var keyedOffTransactions = _transactionHistory?.RecallTransactions<KeyedOffCreditsTransaction>();
                                var lastKeyedOff = keyedOffTransactions?.OrderByDescending(x => x.LogSequence).FirstOrDefault();
                                amount = lastKeyedOff?.TransactionAmount ?? 0;
                                lastTransactionId = lastKeyedOff?.TransactionId ?? 0;
                                break;
                            case SessionEventType.Handpay:
                                var handpayTransactions = _transactionHistory?.RecallTransactions<HandpayTransaction>();
                                var lastHandpay = handpayTransactions?.OrderByDescending(x => x.LogSequence)
                                    .FirstOrDefault();
                                amount = lastHandpay?.TransactionAmount ?? 0;
                                lastTransactionId = lastHandpay?.TransactionId ?? 0;
                                break;
                            case SessionEventType.Wat:
                                var watTransactions = _transactionHistory?.RecallTransactions<WatTransaction>();
                                var lastWat = watTransactions?.OrderByDescending(x => x.LogSequence).FirstOrDefault();
                                amount = lastWat?.TransactionAmount ?? 0;
                                lastTransactionId = lastWat?.TransactionId ?? 0;
                                break;
                            case SessionEventType.VoucherOut:
                                var voucherOutTransactions =
                                    _transactionHistory?.RecallTransactions<VoucherOutTransaction>();
                                var lastVoucherOut = voucherOutTransactions?.OrderByDescending(x => x.LogSequence)
                                    .FirstOrDefault();
                                amount = lastVoucherOut?.TransactionAmount ?? 0;
                                lastTransactionId = lastVoucherOut?.TransactionId ?? 0;
                                break;
                            case SessionEventType.HardMeterOut:
                                var hardMeterOutTransactions =
                                    _transactionHistory?.RecallTransactions<HardMeterOutTransaction>();
                                var lastHardmeterOut = hardMeterOutTransactions?.OrderByDescending(x => x.LogSequence)
                                    .FirstOrDefault();
                                amount = lastHardmeterOut?.TransactionAmount ?? 0;
                                lastTransactionId = lastHardmeterOut?.TransactionId ?? 0;
                                break;
                        }
                    }

                    if (amount > 0 &&
                        _endEventType != SessionEventType.None &&
                        _startTransactionId < lastTransactionId)
                    {
                        return (double)amount.MillicentsToDollars();
                    }
                }

                return 0;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus?.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private void HandleCreditOutEvent(SessionEventType sessionType, long transactionId, long amount)
        {
            Logger.Debug("Handling HandleCreditOutEvent");

            _lastOutAmount = amount;
            _lastTransactionId = transactionId;

            if (_bank.QueryBalance() == 0 && (_gamePlayState?.Idle ?? true))
            {
                TraceEndSession(sessionType, transactionId);
            }
            else
            {
                TraceCashOutDuringSession(sessionType, transactionId);
            }

            _eventBus.Publish(new SessionInfoEvent(GetSessionPaidValue()));
        }

        private void TraceStartSession(long transactionId)
        {
            Logger.Debug($"TraceStartSession Session Event Type: Transaction ID: {transactionId}");
            lock (_lock)
            {
                if (!_sessionStarted)
                {
                    _lastOutAmount = 0;
                    _lastTransactionId = 0;
                    _sessionStarted = true;
                    _cashedOutDuringSession = false;
                    _startTransactionId = transactionId;

                    Logger.Debug("Starting new session");

                    using (var transaction = _accessor.StartTransaction())
                    {
                        transaction[SessionStartedField] = true;
                        transaction[CashedOutDuringSessionField] = false;
                        transaction[StartTransactionIdField] = transactionId;
                        transaction.Commit();
                    }
                }
            }

            _eventBus.Publish(new SessionInfoEvent(GetSessionPaidValue()));
        }

        private void TraceEndSession(SessionEventType eventType, long transactionId)
        {
            Logger.Debug($"TraceEndSession Session Event Type: {eventType}, Transaction ID: {transactionId}");
            lock (_lock)
            {
                if (_sessionStarted)
                {
                    _sessionStarted = false;
                    _endEventType = eventType;
                    _endTransactionId = transactionId;

                    Logger.Debug("Ending session");

                    using (var transaction = _accessor.StartTransaction())
                    {
                        transaction[SessionStartedField] = false;
                        transaction[EndEventTypeField] = eventType;
                        transaction[EndTransactionIdField] = transactionId;
                        transaction.Commit();
                    }
                }
            }
        }

        private void TraceCashOutDuringSession(SessionEventType eventType, long transactionId)
        {
            Logger.Debug("TraceCashOutDuringSession");
            lock (_lock)
            {
                if (_sessionStarted)
                {
                    _cashedOutDuringSession = true;
                    _endEventType = eventType;
                    _endTransactionId = transactionId;

                    Logger.Debug("Cashing Out During Session");

                    using (var transaction = _accessor.StartTransaction())
                    {
                        transaction[CashedOutDuringSessionField] = true;
                        transaction[EndEventTypeField] = eventType;
                        transaction[EndTransactionIdField] = transactionId;
                        transaction.Commit();
                    }
                }
            }
        }

        private void HandleEvent(GameIdleEvent @event)
        {
            if ((_bank?.QueryBalance() ?? 0) == 0)
            {
                Logger.Debug("Detected Game Idle at 0 Bank Balance to End Session");
                // End the session if the balance becomes zero. Use the same transaction Id with the start session.
                TraceEndSession(SessionEventType.BalanceZero, _startTransactionId);
            }
        }
    }
}