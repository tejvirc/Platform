namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Accounting.Contracts;
    using Application.Contracts;
    using Contracts;
    using Contracts.Rtp;
    using Contracts.Session;
    using Hardware.Contracts;
    using Hardware.Contracts.IdReader;
    using Hardware.Contracts.Persistence;
    using Hardware.Contracts.SharedDevice;
    using Kernel;
    using log4net;

    [CLSCompliant(false)]
    public class PlayerService : IPlayerService, IService, IDisposable
    {
        private const PersistenceLevel StorageLevel = PersistenceLevel.Critical;

        private const string StatusField = @"PlayerTracking.Status";
        private const string InitialMetersField = @"Session.InitialMeters";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        private readonly IPersistentStorageAccessor _accessor;
        private readonly IEventBus _bus;
        private readonly IGameHistory _gameHistory;
        private readonly IGamePlayState _gameState;
        private readonly IPlayerSessionHistory _history;
        private readonly IIdProvider _idProvider;
        private readonly IIdReaderProvider _idReaderProvider;
        private readonly IMeterManager _meterManager;
        private readonly ITransactionCoordinator _transactions;
        private readonly IPropertiesManager _properties;
        private readonly IPersistentStorageManager _storageManager;

        private readonly List<AwardParameters> _awardParameters = new List<AwardParameters>();
        private readonly IDictionary<string, long> _meterSnapshot = new ConcurrentDictionary<string, long>();

        private readonly object _delaySync = new object();
        private readonly object _sync = new object();

        private PlayerSession _activeSession;
        private SessionStartDelayTypes _delayType;
        private bool _endOnIdle;
        private IIdReaderValidator _idReaderValidator;
        private PlayerOptions _options;
        private Tuple<Identity, IIdReader> _pendingSession;
        private ReaderWriterLockSlim _sessionLock;
        private PlayerStatus? _status;

        private bool _disposed;

        public PlayerService(
            IEventBus bus,
            IGamePlayState gameState,
            ITransactionCoordinator transactions,
            IGameHistory gameHistory,
            IIdProvider idProvider,
            IPlayerSessionHistory history,
            IPropertiesManager properties,
            IPersistentStorageManager storageManager,
            IMeterManager meterManager,
            IIdReaderProvider idReaderValidator)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
            _transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
            _idProvider = idProvider ?? throw new ArgumentNullException(nameof(idProvider));
            _history = history ?? throw new ArgumentNullException(nameof(history));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _meterManager = meterManager ?? throw new ArgumentNullException(nameof(meterManager));
            _storageManager = storageManager ?? throw new ArgumentNullException(nameof(storageManager));
            _idReaderProvider = idReaderValidator ?? throw new ArgumentNullException(nameof(idReaderValidator));

            _accessor = storageManager.GetAccessor(StorageLevel, GetType().ToString());

            _sessionLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        }

        private IIdReaderValidator PlayerServiceConfiguration
            => _idReaderValidator ?? (_idReaderValidator =
                   ServiceManager.GetInstance().TryGetService<IIdReaderValidator>());

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public bool HasActiveSession => ActiveSession != null;

        /// <inheritdoc />
        public IPlayerSession ActiveSession
        {
            get
            {
                _sessionLock.EnterReadLock();
                try
                {
                    return _activeSession;
                }
                finally
                {
                    _sessionLock.ExitReadLock();
                }
            }
        }

        /// <inheritdoc />
        public bool Enabled => Status == PlayerStatus.None;

        /// <inheritdoc />
        public PlayerStatus Status
        {
            get => (PlayerStatus)(_status ?? (_status = (PlayerStatus)_accessor[StatusField]));

            private set
            {
                if (_status != value)
                {
                    _accessor[StatusField] = _status = value;
                }
            }
        }

        public PlayerOptions Options
        {
            get { return _options ??= _storageManager.GetEntity<PlayerOptions>(); }
            set
            {
                if (value != null)
                {
                    _options = _storageManager.UpdateEntity(value);
                }
            }
        }

        /// <inheritdoc />
        public void SetSessionParameters(long transactionId, long pointBalance, long overrideId)
        {
            if (ActiveSession == null || ActiveSession.Log.TransactionId != transactionId)
            {
                return;
            }

            // This is an update only and only to the players point balance.  It doesn't signify the start of a session
            _sessionLock.EnterWriteLock();
            try
            {
                _activeSession.PointBalance = pointBalance;
                // TODO: Something with the override Id
            }
            finally
            {
                _sessionLock.ExitWriteLock();
            }

            Logger.Info($"Session acknowledged [{transactionId}]");
        }

        /// <inheritdoc />
        public void SetSessionParameters(
            long transactionId,
            long pointBalance,
            long carryOver,
            GenericOverrideParameters overrideParameters)
        {
            if (ActiveSession == null || ActiveSession.Log.TransactionId != transactionId)
            {
                return;
            }

            if (overrideParameters != null)
            {
                AddParams(overrideParameters);
            }

            _sessionLock.EnterWriteLock();
            try
            {
                _activeSession.PointBalance = pointBalance;
                _activeSession.PointCountdown = 0;
                _activeSession.SessionPoints = 0;
            }
            finally
            {
                _sessionLock.ExitWriteLock();
            }

            Logger.Info($"Session acknowledged [{transactionId}]");
        }

        /// <inheritdoc />
        public void SetSessionParameters(Identity identity)
        {
            if (ActiveSession == null)
            {
                return;
            }

            var log = (PlayerSessionLog)ActiveSession.Log;
            _sessionLock.EnterWriteLock();
            try
            {
                _activeSession.Player = identity;
                log.PlayerId = ActiveSession.Player.PlayerId;
                log.TransactionId = _idProvider.GetNextTransactionId();
                log.StartDateTime = DateTime.UtcNow;
                log.EndDateTime = DateTime.UtcNow;
                _activeSession.Log = log;
            }
            finally
            {
                _sessionLock.ExitWriteLock();
            }

            // TODO : Add log to _history?

            _bus.Publish(new SessionUpdatedEvent(log));

            Logger.Info($"Session Player ID updated [{log.TransactionId}]: {log.IdNumber} - {log.PlayerId}");
        }

        public void CommitSession(long transactionId)
        {
            if (_history.GetByTransactionId(transactionId) is PlayerSessionLog log)
            {
                log.PlayerSessionState = PlayerSessionState.SessionAck;

                _history.UpdateLog(log);

                _bus.Publish(new SessionCommittedEvent(log));
            }
        }

        /// <inheritdoc />
        public void Enable(PlayerStatus status)
        {
            if ((Status & status) == 0)
            {
                return;
            }

            Status &= ~status;

            if (Enabled)
            {
                _bus.Publish(new PlayerTrackingEnabledEvent());

                Logger.Info("Player tracking enabled");

                var idReader = _idReaderProvider.Adapters.FirstOrDefault(x => x.Identity != null);
                if (idReader != null)
                {
                    HandleSessionStart(idReader.Identity, idReader);
                }
            }
        }

        /// <inheritdoc />
        public void Disable(PlayerStatus status)
        {
            if ((Status & status) == status)
            {
                return;
            }

            Status |= status;

            _bus.Publish(new PlayerTrackingDisabledEvent());

            Logger.Info($"Player tracking disabled: {Status}");

            HandleSessionEnd();
        }

        /// <inheritdoc />
        public TParam GetParameters<TParam>() where TParam : AwardParameters
        {
            lock (_sync)
            {
                return (TParam)_awardParameters.FirstOrDefault(param => param is TParam);
            }
        }

        /// <inheritdoc />
        public string Name => typeof(PlayerService).ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IPlayerService) };

        /// <inheritdoc />
        public void Initialize()
        {
            // TODO: Need to watch for property changes or just store the params
            AddParams(new BaseParameters(Options.BaseTarget, Options.BaseIncrement, Options.BaseAward));

            _bus.Subscribe<SetValidationEvent>(this, Handle);
            _bus.Subscribe<GameIdleEvent>(this, Handle);
            _bus.Subscribe<DisabledEvent>(this, Handle);
            _bus.Subscribe<TransactionCompletedEvent>(this, Handle);
            _bus.Subscribe<IdReaderTimeoutEvent>(this, Handle);
            _bus.Subscribe<ValidationDeviceChangedEvent>(this, Handle);

            LoadSnapshot();

            _sessionLock.EnterWriteLock();
            try
            {
                if (_history.CurrentLog?.PlayerSessionState == PlayerSessionState.SessionOpen)
                {
                    Logger.Info($"Active session {_history.CurrentLog.TransactionId} found during init");

                    _activeSession = new PlayerSession
                    {
                        Log = _history.CurrentLog,
                        Player = new Identity(),
                        PointBalance = 0
                    };

                    HandleSessionEnd();
                }
            }
            finally
            {
                _sessionLock.ExitWriteLock();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (_sessionLock != null)
                {
                    _sessionLock.Dispose();
                }

                _bus.UnsubscribeAll(this);
            }

            _sessionLock = null;

            _disposed = true;
        }

        private void Handle(SetValidationEvent evt)
        {
            if (PlayerServiceConfiguration != null && !PlayerServiceConfiguration.CanAccept(evt.IdReaderId))
            {
                if (evt.Identity != null)
                {
                    _bus.Publish(new StartSessionFailedEvent(evt.IdReaderId));
                    Logger.Info($"Cannot start player session for readerId:{evt.IdReaderId}");
                }
                return;
            }

            if (!Enabled)
            {
                Logger.Info($"Validation event ignored. Sessions are disabled: {Status}");
                return;
            }

            if (evt.Identity == null)
            {
                HandleSessionEnd();
            }
            else
            {
                var idReader = _idReaderProvider.Adapters.Single(id => id.IdReaderId == evt.IdReaderId);

                HandleSessionStart(evt.Identity, idReader);
            }
        }

        private void Handle(GameIdleEvent evt)
        {
            if (ActiveSession == null)
            {
                HandlePendingSession(SessionStartDelayTypes.GameState);

                return;
            }

            var log = (PlayerSessionLog)ActiveSession.Log;

            _sessionLock.EnterWriteLock();
            try
            {
                var game = _properties.GetValues<IGameDetail>(GamingConstants.Games)
                    .FirstOrDefault(g => g.Id == evt.GameId);
                if (game != null)
                {
                    // TODO: If any of these attributes changes we'll need to generate an interval rating

                    log.ThemeId = game.ThemeId;
                    log.PaytableId = game.PaytableId;
                    log.DenomId = evt.Denomination;

                    CalculateGameEndMeters(evt.Log, log);
                }

                log.EndDateTime = DateTime.UtcNow;

                UpdateMeters(log);
            }
            finally
            {
                _sessionLock.ExitWriteLock();
            }

            _bus.Publish(new SessionUpdatedEvent(log));

            if (_endOnIdle)
            {
                HandleSessionEnd();
            }
        }

        private void Handle(DisabledEvent evt)
        {
            if ((evt.Reasons & DisabledReasons.Backend) == DisabledReasons.Backend ||
                (evt.Reasons & DisabledReasons.Configuration) == DisabledReasons.Configuration ||
                (evt.Reasons & DisabledReasons.Operator) == DisabledReasons.Operator)
            {
                Logger.Debug($"Forcibly ending the active player session due to Id Reader disabled: {evt.Reasons}");

                HandleSessionEnd();
            }
        }

        private void Handle(TransactionCompletedEvent evt)
        {
            if (_endOnIdle)
            {
                HandleSessionEnd();
            }

            HandlePendingSession(SessionStartDelayTypes.Transaction);
        }

        private void Handle(IdReaderTimeoutEvent evt)
        {
            if (Options.InactiveSessionEnd && HasActiveSession)
            {
                Logger.Info($"Ending session due to reader {evt.IdReaderId} timeout event");
                HandleSessionEnd();
            }
        }

        private void Handle(ValidationDeviceChangedEvent evt)
        {
            var idReader = _idReaderProvider.Adapters.FirstOrDefault(x => x.Identity != null);

            if (idReader != null && HasActiveSession)
            {
                if (!_idReaderValidator.CanAccept(idReader.IdReaderId))
                {
                    Logger.Info($"Ending session due to validation device configuration change. Id {idReader.IdReaderId} is no longer valid");
                    HandleSessionEnd();
                }
            }
        }

        private void AddParams<TParam>(TParam param) where TParam : AwardParameters
        {
            lock (_sync)
            {
                _awardParameters.RemoveAll(p => p is TParam);

                _awardParameters.Add(param);
            }
        }

        private void RemoveParams<TParam>() where TParam : AwardParameters
        {
            lock (_sync)
            {
                _awardParameters.RemoveAll(p => p is TParam);
            }
        }

        private void HandlePendingSession(SessionStartDelayTypes type)
        {
            lock (_delaySync)
            {
                _delayType &= ~type;
                if (_delayType == SessionStartDelayTypes.None && _pendingSession != null)
                {
                    HandleSessionStart(_pendingSession.Item1, _pendingSession.Item2);
                }
            }
        }

        private void HandleSessionStart(Identity identity, IIdReader reader)
        {
            _sessionLock.EnterWriteLock();
            try
            {
                if (identity?.Type != IdTypes.Player || identity.State != IdStates.Active)
                {
                    _pendingSession = null;
                    HandleSessionEnd();
                    return;
                }

                lock (_delaySync)
                {
                    if (SetSessionDelay() != SessionStartDelayTypes.None)
                    {
                        Logger.Info("Session start delayed until game end and open transactions are closed");

                        // If we're in a game round or we have an open transaction
                        //  we need to delay the session start until the end of the current session
                        _pendingSession = Tuple.Create(identity, reader);
                        return;
                    }
                }

                var log = new PlayerSessionLog
                {
                    TransactionId = _idProvider.GetNextTransactionId(),
                    IdReaderType = reader.IdReaderType,
                    IdNumber = identity.Number,
                    PlayerId = identity.PlayerId,
                    PlayerSessionState = PlayerSessionState.SessionOpen,
                    StartDateTime = DateTime.UtcNow,
                    EndDateTime = DateTime.UtcNow
                };

                GetInitialMeters();

                _history.AddLog(log);

                _activeSession = new PlayerSession
                {
                    Player = identity,
                    PointBalance = 0,
                    Log = log
                };

                _bus.Publish(new SessionStartedEvent(log));

                Logger.Info($"Session started [{log.TransactionId}]: {log.IdNumber} - {log.PlayerId}");
            }
            finally
            {
                _sessionLock.ExitWriteLock();
            }
        }

        private void HandleSessionEnd()
        {
            _sessionLock.EnterWriteLock();
            try
            {
                if (ActiveSession == null)
                {
                    return;
                }

                // Session end must be delayed until the game state is idle and there are no open transactions
                if (!_gameState.Idle || _gameHistory.IsRecoveryNeeded || _transactions.IsTransactionActive)
                {
                    _endOnIdle = true;
                    return;
                }

                var log = (PlayerSessionLog)ActiveSession.Log;

                FinalizeLog(log);

                _activeSession = null;

                RemoveParams<SessionParameters>();

                _bus.Publish(new SessionEndedEvent(log));

                _endOnIdle = false;

                Logger.Info($"Session ended [{log.TransactionId}]: {log.IdNumber} - {log.PlayerId}");

                if (_pendingSession != null)
                {
                    HandleSessionStart(_pendingSession.Item1, _pendingSession.Item2);
                    _pendingSession = null;
                }
            }
            finally
            {
                _sessionLock.ExitWriteLock();
            }
        }

        private void FinalizeLog(PlayerSessionLog log)
        {
            log.PlayerSessionState = PlayerSessionState.SessionCommit;
            log.EndDateTime = DateTime.UtcNow;

            UpdateMeters(log);

            _history.UpdateLog(log);
        }

        private void UpdateMeters(PlayerSessionLog log)
        {
            var delta = _meterManager.GetSnapshotDelta(_meterSnapshot, MeterValueType.Lifetime);

            log.SessionMeters = delta.Select(m => new SessionMeter { Name = m.Key, Value = m.Value }).ToList();

            log.WonCount = GetMeterDelta(delta, GamingMeters.WonCount);
            log.LostCount = GetMeterDelta(delta, GamingMeters.LostCount);
            log.TiedCount = GetMeterDelta(delta, GamingMeters.TiedCount);
            log.TheoreticalPaybackAmount = GetMeterDelta(delta, GamingMeters.TheoPayback);
            log.WageredCashableAmount = GetMeterDelta(delta, GamingMeters.WageredCashableAmount);
            log.WageredNonCashAmount = GetMeterDelta(delta, GamingMeters.WageredNonCashableAmount);
            log.WageredPromoAmount = GetMeterDelta(delta, GamingMeters.WageredPromoAmount);
            log.EgmPaidGameWonAmount = GetMeterDelta(delta, GamingMeters.TotalEgmPaidGameWonAmount);
            log.EgmPaidProgWonAmount = GetMeterDelta(delta, GamingMeters.EgmPaidProgWonAmount);
        }

        private long GetMeterDelta(IDictionary<string, long> meters, string meter)
        {
            return !meters.TryGetValue(meter, out var value) ? 0 : value;
        }

        private void LoadSnapshot()
        {
            var meters = _accessor.GetList<SessionMeter>(InitialMetersField);

            foreach (var meter in meters)
            {
                _meterSnapshot.Add(meter.Name, meter.Value);
            }
        }

        private void GetInitialMeters()
        {
            _meterSnapshot.Clear();

            var snapshot = _meterManager.CreateSnapshot(MeterValueType.Lifetime);

            foreach (var meter in snapshot)
            {
                _meterSnapshot.Add(meter);
            }

            using (var transaction = _accessor.StartTransaction())
            {
                transaction.UpdateList(
                    InitialMetersField,
                    snapshot.Select(m => new SessionMeter { Name = m.Key, Value = m.Value }).ToList());

                transaction.Commit();
            }
        }

        private void CalculateGameEndMeters(IGameHistoryLog gameLog, PlayerSessionLog log)
        {
            // This is a player specific meter, so we're going to calculate it here
            var wagerCategory = _properties.GetValue<IWagerCategory>(GamingConstants.SelectedWagerCategory, null);
            if (wagerCategory == null)
            {
                return;
            }

            const decimal oneHundredPercent = 100;

            var hold = Convert.ToDouble(
                gameLog.FinalWager * GamingConstants.Millicents * Math.Max(
                    oneHundredPercent - wagerCategory.TheoPaybackPercent,
                    Options.MinimumTheoreticalHoldPercentageMeter.FromMeter()) / oneHundredPercent);

            log.TheoreticalHoldAmount += (long)Math.Round(hold, MidpointRounding.ToEven);
        }

        private SessionStartDelayTypes SetSessionDelay()
        {
            _sessionLock.EnterWriteLock();

            if (_delayType == SessionStartDelayTypes.None)
            {
                if (!_gameState.Idle)
                {
                    _delayType |= SessionStartDelayTypes.GameState;
                }
                if (_transactions.IsTransactionActive)
                {
                    _delayType |= SessionStartDelayTypes.Transaction;
                }
            }

            _sessionLock.ExitWriteLock();
            return _delayType;
        }

        [Flags]
        private enum SessionStartDelayTypes
        {
            None,
            GameState,
            Transaction
        }

        private class PlayerSession : IPlayerSession
        {
            public Identity Player { get; set; }
            public DateTime Start => Log.StartDateTime;
            public DateTime End => Log.EndDateTime;
            public long PointBalance { get; set; }
            public long PointCountdown { get; set; }
            public long SessionPoints { get; set; }
            public IPlayerSessionLog Log { get; set; }
        }
    }
}
