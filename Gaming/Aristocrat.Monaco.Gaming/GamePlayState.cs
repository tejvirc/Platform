namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Commands;
    using Contracts;
    using Contracts.Bonus;
    using Contracts.Central;
    using Contracts.Progressives;
    using Kernel;
    using log4net;
    using Stateless;

    /// <summary>
    ///     An implementation of <see cref="IGamePlayState" />.
    /// </summary>
    [CLSCompliant(false)]
    public class GamePlayState : IGamePlayState, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IEventBus _eventBus;
        private readonly IGameHistory _gameHistory;
        private readonly ICommandHandlerFactory _handlerFactory;
        private readonly IPropertiesManager _properties;
        private readonly ISystemDisableManager _systemDisableManager;
        private readonly ITransferOutHandler _transferHandler;
        private readonly IMoneyLaunderingMonitor _moneyLaunderingMonitor;

        private readonly object _gameDelayLock = new();
        private readonly Stopwatch _gamePlayDuration = new();

        private ReaderWriterLockSlim _stateLock;

        private int _gameId;
        private long _denom;
        private IWagerCategory _wagerCategory;
        private bool _enabled;
        private bool _faulted;
        private bool _pendingEvents;

        private StateMachine<PlayState, Trigger>.TriggerWithParameters<long> _payResultTrigger;
        private StateMachine<PlayState, Trigger>.TriggerWithParameters<long, byte[], IOutcomeRequest> _primaryGameEscrowTrigger;
        private StateMachine<PlayState, Trigger>.TriggerWithParameters<long, byte[]> _primaryGameStartTrigger;
        private StateMachine<PlayState, Trigger>.TriggerWithParameters<long> _secondaryGameStartTrigger;
        private StateMachine<PlayState, Trigger> _state;

        private CancellationTokenSource _delayTimerCancellationToken;
        private bool _gameDelayExpired;
        private bool _platformHeldGameEnd;

        private bool _disposed;

        public GamePlayState(
            IEventBus eventBus,
            ISystemDisableManager systemDisableManager,
            ICommandHandlerFactory handlerFactory,
            IGameHistory gameHistory,
            IPropertiesManager properties,
            ITransferOutHandler transferHandler,
            IMoneyLaunderingMonitor moneyLaunderingMonitor)
        {
            _eventBus = eventBus
                ?? throw new ArgumentNullException(nameof(eventBus));
            _systemDisableManager = systemDisableManager
                ?? throw new ArgumentNullException(nameof(systemDisableManager));
            _handlerFactory = handlerFactory
                ?? throw new ArgumentNullException(nameof(handlerFactory));
            _gameHistory = gameHistory
                ?? throw new ArgumentNullException(nameof(gameHistory));
            _properties = properties
                ?? throw new ArgumentNullException(nameof(properties));
            _transferHandler = transferHandler
                ?? throw new ArgumentNullException(nameof(transferHandler));
            _moneyLaunderingMonitor = moneyLaunderingMonitor
                ?? throw new ArgumentNullException(nameof(moneyLaunderingMonitor));

            _stateLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

            CreateStateMachine(PlayState.Idle);

            GameDelay = TimeSpan.Zero;

            Initialize();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public TimeSpan GameDelay { get; private set; }

        /// <inheritdoc />
        public bool Enabled
        {
            get
            {
                _stateLock.EnterReadLock();
                try
                {
                    return _enabled;
                }
                finally
                {
                    _stateLock.ExitReadLock();
                }
            }
            private set
            {
                _stateLock.EnterWriteLock();
                try
                {
                    _enabled = value;
                }
                finally
                {
                    _stateLock.ExitWriteLock();
                }
            }
        }

        /// <inheritdoc />
        public bool Idle
        {
            get
            {
                _stateLock?.EnterReadLock();
                try
                {
                    return _state.IsInState(PlayState.Idle);
                }
                finally
                {
                    _stateLock?.ExitReadLock();
                }
            }
        }

        public bool InPresentationIdle
        {
            get
            {
                _stateLock?.EnterReadLock();
                try
                {
                    return _state.IsInState(PlayState.PresentationIdle);
                }
                finally
                {
                    _stateLock?.ExitReadLock();
                }
            }
        }

        /// <inheritdoc />
        public bool InGameRound => !Idle;

        /// <inheritdoc />
        public PlayState CurrentState
        {
            get
            {
                _stateLock.EnterReadLock();
                try
                {
                    return _state.State;
                }
                finally
                {
                    _stateLock.ExitReadLock();
                }
            }
        }

        /// <inheritdoc />
        public PlayState UncommittedState => _state.State;

        /// <inheritdoc />
        public string Name => typeof(GamePlayState).ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IGamePlayState) };

        private bool GameDelayExpired
        {
            get
            {
                lock (_gameDelayLock)
                {
                    return _gameDelayExpired;
                }
            }
            set
            {
                lock (_gameDelayLock)
                {
                    _gameDelayExpired = value;
                }
            }
        }

        private bool AllowGamePlayOnNormalLockup => _properties.GetValue(GamingConstants.AdditionalInfoGameInProgress, false);

        /// <inheritdoc />
        public void Initialize()
        {
            _eventBus.Subscribe<GameProcessExitedEvent>(this, _ => Fire(Trigger.ProcessExited));
            _eventBus.Subscribe<ProgressiveHitEvent>(this, _ => Fire(Trigger.ProgressiveHit));
            _eventBus.Subscribe<BonusCommitCompletedEvent>(this, _ => End(-1));
            _eventBus.Subscribe<OutcomeFailedEvent>(this, _ => Fire(Trigger.PrimaryGameRequestFailed));
            _eventBus.Subscribe<GameLoadedEvent>(this, _ => HandleLoaded());
            _eventBus.Subscribe<SystemDisableAddedEvent>(this, HandleDisable);
            _eventBus.Subscribe<SystemDisableRemovedEvent>(this, HandleEnable);
            _eventBus.Subscribe<SystemEnabledEvent>(this, _ => HandleEnabled());

            Enabled = !_systemDisableManager.DisableImmediately;

            Logger.Debug("Initialized and configured the game play state machine.");
        }

        /// <inheritdoc />
        public bool Prepare()
        {
            return CheckGameRoundStartTimer() && Fire(Trigger.PlayInitiated, true) && _state.IsInState(PlayState.Initiated);
        }

        /// <inheritdoc />
        public bool EscrowWager(long initialWager, byte[] data, IOutcomeRequest request, bool recovering)
        {
            SetGamePlayData();

            if (recovering)
            {
                // Restore the state we left off from
                Logger.Info($"Recovering to state: {_gameHistory.LastPlayState}");

                CreateStateMachine(_gameHistory.LastPlayState);
            }

            FirePrimaryGameEscrow(initialWager, data, request);

            return !_state.IsInState(PlayState.Idle);
        }

        /// <inheritdoc />
        public void Start(long initialWager, byte[] data, bool recovering)
        {
            SetGamePlayData();

            if (recovering)
            {
                // If we recovered into EscrowWager state we've already re-initialized the state machine
                if (CurrentState == PlayState.Idle)
                {
                    // Restore the state we left off from
                    Logger.Info($"Recovering to state: {_gameHistory.LastPlayState}");

                    CreateStateMachine(_gameHistory.LastPlayState);
                }

                StartGameDelayTimer();

                if (CurrentState == PlayState.Initiated || CurrentState == PlayState.PrimaryGameEscrow)
                {
                    FirePrimaryGameStart(initialWager, data);
                }
            }
            else
            {
                FirePrimaryGameStart(initialWager, data);
            }
        }

        /// <inheritdoc />
        public void StartSecondaryGame(long stake, long win, bool recovering = false)
        {
            // In a recovery situation we load from the game history record which will be left in the secondary game ended stated so if we end here we have persisted the data.
            if (recovering && CurrentState == PlayState.SecondaryGameEnded)
            {
                return;
            }

            // We don't have enough info from the runtime to do this when it occurs in the game so we're going to force it here if needed
            Fire(Trigger.SecondaryGameChoice, true);

            FireSecondaryGameStart(stake);

            _handlerFactory.Create<SecondaryGameEnded>().Handle(new SecondaryGameEnded(win, stake));

            _eventBus.Publish(new SecondaryGameEndedEvent(_gameId, _denom, _wagerCategory.Id, _gameHistory.CurrentLog, win));
        }

        /// <inheritdoc />
        public void End(long finalWin)
        {
            if (_state.CanFire(_payResultTrigger.Trigger))
            {
                // This is the one state change we can run async, but it's only because we control when the next game round can start.
                //  If the runtime workflow ever changes this will need to be evaluated.
                Task.Run(() => FirePayResults(finalWin));
            }
            else if (_state.CanFire(Trigger.GameEnded))
            {
                if (_pendingEvents)
                {
                    _pendingEvents = false;
                    HandleBonusEvents(Trigger.GameEnded);
                }
                else
                {
                    Fire(Trigger.GameEnded);
                }
            }
            else if (_state.CanFire(Trigger.GameIdle))
            {
                // This state will likely only fire during recovery if we got to the point of game end, but not idle
                Fire(Trigger.GameIdle);
            }
        }

        /// <inheritdoc />
        public void Faulted()
        {
            _stateLock.EnterWriteLock();
            try
            {
                _faulted = true;
            }
            finally
            {
                _stateLock.ExitWriteLock();
            }
        }

        /// <inheritdoc />
        public void SetGameEndDelay(TimeSpan delay)
        {
            GameDelay = delay;

            // Not in a current delay - just set it
            if (CurrentState != PlayState.GameEnded)
            {
                return;
            }

            // Cancel the delay by ending it or extend the delay by starting a new one
            if (GameDelay == TimeSpan.Zero)
            {
                ForceEndGameDelayTimer();
            }
            else
            {
                StartGameDelayTimer();
            }
        }

        public void SetGameEndHold(bool preventGameIdle)
        {
            _platformHeldGameEnd = preventGameIdle;
            if (!preventGameIdle)
            {
                Fire(Trigger.GameIdle, true);
            }
        }

        /// <summary>
        ///     Releases allocated resources.
        /// </summary>
        /// <param name="disposing">
        ///     true to release both managed and unmanaged resources; false to release only unmanaged
        ///     resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);

                if (_delayTimerCancellationToken != null)
                {
                    _delayTimerCancellationToken.Cancel(false);
                    _delayTimerCancellationToken.Dispose();
                }

                _stateLock.Dispose();
            }

            _delayTimerCancellationToken = null;
            _stateLock = null;

            _disposed = true;
        }

        private void StartGameDelayTimer()
        {
            EndGameDelayTimer();

            GameDelayExpired = false;

            if (GameDelay == TimeSpan.Zero)
            {
                GameDelayExpired = true;
                return;
            }

            _eventBus.Publish(new GameDelayStartedEvent());

            _delayTimerCancellationToken = new CancellationTokenSource();

            Task.Delay(GameDelay, _delayTimerCancellationToken.Token)
                .ContinueWith(
                    task =>
                    {
                        if (!task.IsCanceled)
                        {
                            CompleteGameDelay();
                        }
                    });
        }

        private void CompleteGameDelay()
        {
            GameDelayExpired = true;

            _eventBus.Publish(new GameDelayEndedEvent());

            Fire(Trigger.GameIdle, true);
        }

        private void EndGameDelayTimer()
        {
            _delayTimerCancellationToken?.Cancel(false);
            _delayTimerCancellationToken?.Dispose();
            _delayTimerCancellationToken = null;
        }

        private void ForceEndGameDelayTimer()
        {
            EndGameDelayTimer();
            CompleteGameDelay();
        }

        private void HandleDisable(SystemDisableAddedEvent added)
        {
            if (Enabled && (added.Priority == SystemDisablePriority.Immediate || Idle && !_gameHistory.IsRecoveryNeeded) && (!AllowGamePlayOnNormalLockup || added.Priority == SystemDisablePriority.Immediate))
            {
                HandleDisabled();
                return;
            }

            // handle special case of immediate disable while possibly showing presentation override
            if (!Enabled && added.Priority == SystemDisablePriority.Immediate)
            {
                _handlerFactory.Create<SystemDisablesChanged>().Handle(new SystemDisablesChanged());
            }
        }

        private void HandleEnable(SystemDisableRemovedEvent removed)
        {
            if (removed.Priority == SystemDisablePriority.Immediate &&
                _systemDisableManager.IsDisabled && !_systemDisableManager.DisableImmediately &&
                !Enabled && (!Idle || _gameHistory.IsRecoveryNeeded))
            {
                HandleEnabled();
                return;
            }

            // handle special case of removing immediate disable while possibly having a pending presentation override
            if (removed.Priority == SystemDisablePriority.Immediate && _systemDisableManager.IsDisabled)
            {
                _handlerFactory.Create<SystemDisablesChanged>().Handle(new SystemDisablesChanged());
            }
        }

        private void HandleEnabled()
        {
            Enabled = true;
            _handlerFactory.Create<GamePlayEnabled>().Handle(new GamePlayEnabled());
            _eventBus.Publish(new GamePlayEnabledEvent());
        }

        private void HandleDisabled()
        {
            Enabled = false;
            _handlerFactory.Create<GamePlayDisabled>().Handle(new GamePlayDisabled());
            _eventBus.Publish(new GamePlayDisabledEvent());
        }

        private void HandleLoaded()
        {
            Logger.Debug($"HandleLoaded system disabled? {_systemDisableManager.DisableImmediately}");
            if (_systemDisableManager.DisableImmediately && !_gameHistory.IsDiagnosticsActive)
            {
                HandleDisabled();
            }
        }

        private void CreateStateMachine(PlayState initialState)
        {
            _stateLock.EnterWriteLock();
            try
            {
                _faulted = false;
                _pendingEvents = false;
                _state = new StateMachine<PlayState, Trigger>(initialState);

                _eventBus.Publish(new GamePlayStateInitializedEvent(initialState));

                _primaryGameEscrowTrigger = _state.SetTriggerParameters<long, byte[], IOutcomeRequest>(Trigger.PrimaryGameRequested);
                _primaryGameStartTrigger = _state.SetTriggerParameters<long, byte[]>(Trigger.PrimaryGameStarted);
                _secondaryGameStartTrigger = _state.SetTriggerParameters<long>(Trigger.SecondaryGameStarted);
                _payResultTrigger = _state.SetTriggerParameters<long>(Trigger.GameResult);
            }
            finally
            {
                _stateLock.ExitWriteLock();
            }

            _state.Configure(PlayState.Idle)
                .OnEntry(OnIdle)
                .OnExit(OnPlayInitiated)
                .PermitIf(Trigger.PlayInitiated, PlayState.Initiated, () => (!_systemDisableManager.IsDisabled || AllowGamePlayOnNormalLockup) && Enabled);

            _state.Configure(PlayState.Initiated)
                .Permit(Trigger.ProcessExited, PlayState.Idle)
                .Permit(Trigger.InitializationFailed, PlayState.Idle)
                .Permit(Trigger.PrimaryGameRequested, PlayState.PrimaryGameEscrow)
                .Permit(Trigger.PrimaryGameStarted, PlayState.PrimaryGameStarted);

            _state.Configure(PlayState.PrimaryGameEscrow)
                .OnEntryFrom(_primaryGameEscrowTrigger, OnPrimaryGameEscrowed)
                .PermitReentry(Trigger.PrimaryGameRequested)
                .Permit(Trigger.PrimaryGameRequestFailed, PlayState.Idle)
                .Permit(Trigger.PrimaryGameStarted, PlayState.PrimaryGameStarted);

            _state.Configure(PlayState.PrimaryGameStarted)
                .OnEntryFrom(_primaryGameStartTrigger, OnPrimaryGameStarted)
                .OnExit(StartGameDelayTimer)
                .PermitReentry(Trigger.WagerChanged)
                .Permit(Trigger.ProgressiveHit, PlayState.ProgressivePending)
                .Permit(Trigger.SecondaryGameChoice, PlayState.SecondaryGameChoice)
                .Permit(Trigger.GameEnded, PlayState.GameEnded)
                .PermitDynamic(
                    _payResultTrigger,
                    win => win == 0 ? PlayState.GameEnded : PlayState.PayGameResults);

            _state.Configure(PlayState.ProgressivePending)
                .Permit(Trigger.PrimaryGameStarted, PlayState.PrimaryGameStarted)
                .Permit(Trigger.SecondaryGameChoice, PlayState.SecondaryGameChoice)
                .PermitDynamic(
                    _payResultTrigger,
                    win => win == 0 ? PlayState.GameEnded : PlayState.PayGameResults);

            _state.Configure(PlayState.PrimaryGameEnded)
                .OnEntry(() => { })
                .Permit(Trigger.SecondaryGameChoice, PlayState.SecondaryGameChoice)
                .Permit(Trigger.GameResult, PlayState.PayGameResults)
                .Permit(Trigger.GameEnded, PlayState.GameEnded);

            _state.Configure(PlayState.SecondaryGameChoice)
                .OnEntry(OnSecondaryGameChoice)
                .Permit(Trigger.SecondaryGameRequested, PlayState.SecondaryGameEscrow)
                .Permit(Trigger.SecondaryGameStarted, PlayState.SecondaryGameStarted)
                .PermitDynamic(
                    _payResultTrigger,
                    win => win == 0 ? PlayState.GameEnded : PlayState.PayGameResults);

            _state.Configure(PlayState.SecondaryGameEscrow)
                .OnEntry(() => { })
                .Permit(Trigger.SecondaryGameRequestFailed, PlayState.SecondaryGameChoice)
                .Permit(Trigger.SecondaryGameStarted, PlayState.SecondaryGameStarted);

            _state.Configure(PlayState.SecondaryGameStarted)
                .OnEntryFrom(_secondaryGameStartTrigger, OnSecondaryGameStart)
                .OnExit(StartGameDelayTimer)
                .Permit(Trigger.SecondaryGameChoice, PlayState.SecondaryGameChoice)
                .PermitDynamic(
                    _payResultTrigger,
                    win => win == 0 ? PlayState.GameEnded : PlayState.PayGameResults);

            _state.Configure(PlayState.SecondaryGameEnded)
                .OnEntry(() => { })
                .Permit(Trigger.SecondaryGameChoice, PlayState.SecondaryGameChoice)
                .Permit(Trigger.GameResult, PlayState.PayGameResults)
                .Permit(Trigger.GameEnded, PlayState.GameEnded);

            _state.Configure(PlayState.PayGameResults)
                .OnEntryFrom(_payResultTrigger, OnPayGameResult)
                .OnEntry(() => { })
                .OnExit(() => _eventBus.Publish(new GameResultEvent(_gameId, _denom, _wagerCategory.Id, _gameHistory.CurrentLog)))
                .PermitIf(Trigger.GameEnded, PlayState.GameEnded, () => !_faulted && !_transferHandler.InProgress);

            _state.Configure(PlayState.GameEnded)
                .OnEntry(OnGameEnded)
                .OnExit(OnGameEndedExit)
                .PermitDynamicIf(
                    Trigger.GameIdle,
                    () => _platformHeldGameEnd ? PlayState.PresentationIdle : PlayState.Idle,
                    () => GameDelayExpired && !_faulted);

            _state.Configure(PlayState.PresentationIdle)
                .OnEntry(OnGameIdleHeld)
                .OnExit(OnGameIdleHeldExit)
                .PermitIf(
                    Trigger.PlayInitiated,
                    PlayState.Idle,
                    () => (!_systemDisableManager.IsDisabled || AllowGamePlayOnNormalLockup) && Enabled && !_faulted)
                .PermitIf(Trigger.GameIdle, PlayState.Idle, () => !_platformHeldGameEnd && !_faulted);

            _state.OnUnhandledTrigger(
                (state, trigger) =>
                {
                    Logger.Error(
                        $"Invalid Game Play State Transition. State : {state} Trigger : {trigger}");
                });

            _state.OnTransitioned(
                transition =>
                {
                    _eventBus.Publish(new GamePlayStateChangedEvent(transition.Source, transition.Destination));

                    Logger.Debug(
                        $"Transitioned From : {transition.Source} To : {transition.Destination} Trigger : {transition.Trigger}");
                });
        }

        private void OnGameIdleHeldExit(StateMachine<PlayState, Trigger>.Transition transition)
        {
            _eventBus.Publish(
                new GameEndedEvent(_gameId, _denom, _wagerCategory.Id, _gameHistory.CurrentLog));
            _gameId = -1;
            _denom = -1;
            if (transition.Trigger != Trigger.PlayInitiated)
            {
                return;
            }

            // Re-fire the play initiated so we correctly flow through all game play states
            Fire(Trigger.PlayInitiated);
        }

        private void OnGameIdleHeld()
        {
            // Handle unlock and handle bonuses
            _handlerFactory.Create<PresentationIdle>().Handle(new PresentationIdle());
            HandleBonusEvents(Trigger.GameIdle);
        }

        private void OnGameEndedExit(StateMachine<PlayState, Trigger>.Transition transition)
        {
            if (transition.Destination == PlayState.PresentationIdle)
            {
                return;
            }

            _eventBus.Publish(
                new GameEndedEvent(_gameId, _denom, _wagerCategory.Id, _gameHistory.CurrentLog));
        }

        private void OnPlayInitiated()
        {
            EndGameDelayTimer();

            Logger.Debug("Prepare for game round started");

            var initiated = new GamePlayInitiated();
            _handlerFactory.Create<GamePlayInitiated>().Handle(initiated);
            if (!initiated.Success)
            {
                Logger.Warn("Prepare for game round failed");
                Fire(Trigger.InitializationFailed);
                return;
            }

            Logger.Debug("Game play initiated");

            _eventBus.Publish(new GamePlayInitiatedEvent());
        }

        private void OnIdle(StateMachine<PlayState, Trigger>.Transition transition)
        {
            _faulted = false;

            GameDelay = TimeSpan.Zero;

            if (_systemDisableManager.IsDisabled)
            {
                HandleDisabled();
            }

            if (transition.Source == PlayState.Initiated)
            {
                _handlerFactory.Create<GameRoundFailed>().Handle(new GameRoundFailed());
            }
            else if (transition.Source != PlayState.PrimaryGameEscrow)
            {
                _handlerFactory.Create<GameIdle>().Handle(new GameIdle());
                _eventBus.Publish(new GameIdleEvent(_gameId, _denom, _wagerCategory.Id, _gameHistory.CurrentLog));

                HandleBonusEvents(Trigger.GameIdle);
            }
            else if (transition.Source == PlayState.PrimaryGameEscrow)
            {
                _handlerFactory.Create<PrimaryGameEscrowFailed>().Handle(new PrimaryGameEscrowFailed());
                _eventBus.Publish(new PrimaryGameFailedEvent(_gameId, _denom, _wagerCategory.Id, _gameHistory.CurrentLog));

                HandleBonusEvents(Trigger.GameIdle);
            }

            _gameId = -1;
            _denom = -1;
        }

        private void OnPrimaryGameEscrowed(long initialWager, byte[] data, IOutcomeRequest request)
        {
            var command = new PrimaryGameEscrow(initialWager, data, request);
            _handlerFactory.Create<PrimaryGameEscrow>().Handle(command);

            _eventBus.Publish(new PrimaryGameEscrowEvent(_gameId, _denom, _wagerCategory.Id, _gameHistory.CurrentLog));

            if (!command.Result)
            {
                Fire(Trigger.PrimaryGameRequestFailed);
            }
        }

        private void OnPrimaryGameStarted(long wager, byte[] data)
        {
            _gamePlayDuration.Restart();
            _handlerFactory.Create<PrimaryGameStarted>().Handle(new PrimaryGameStarted(_gameId, _denom, wager, data));

            _eventBus.Publish(new PrimaryGameStartedEvent(_gameId, _denom, _wagerCategory.Id, _gameHistory.CurrentLog));

            _moneyLaunderingMonitor.NotifyGameStarted();
        }

        private void OnSecondaryGameChoice(StateMachine<PlayState, Trigger>.Transition transition)
        {
            HandleGameEndTransition(transition.Source, _gameHistory.CurrentLog.UncommittedWin);

            _handlerFactory.Create<SecondaryGameChoice>().Handle(new SecondaryGameChoice());

            _eventBus.Publish(new SecondaryGameChoiceEvent(_gameId, _denom, _wagerCategory.Id, _gameHistory.CurrentLog));
        }

        private void OnSecondaryGameStart(long stake, StateMachine<PlayState, Trigger>.Transition transition)
        {
            _handlerFactory.Create<SecondaryGameStarted>().Handle(new SecondaryGameStarted(stake));

            _eventBus.Publish(new SecondaryGameStartedEvent(_gameId, _denom, _wagerCategory.Id, _gameHistory.CurrentLog, stake));
        }

        private void OnPayGameResult(long win, StateMachine<PlayState, Trigger>.Transition transition)
        {
            HandleGameEndTransition(transition.Source, win);

            var results = new PayGameResults(win);

            _handlerFactory.Create<PayGameResults>().Handle(results);

            // If there are transactions pending, bonus events (or any other type for that matter) need to be delayed
            _pendingEvents = results.PendingTransaction;
            if (!_pendingEvents)
            {
                HandleBonusEvents(Trigger.GameEnded);
            }
        }

        private void OnGameEnded(StateMachine<PlayState, Trigger>.Transition transition)
        {
            HandleGameEndTransition(transition.Source, 0);

            var command = new GameEnded();
            _handlerFactory.Create<GameEnded>().Handle(command);

            HandleBonusEvents(Trigger.GameIdle);
        }

        private void HandleGameEndTransition(PlayState source, long win)
        {
            switch (source)
            {
                case PlayState.PrimaryGameStarted:
                case PlayState.ProgressivePending:
                    _handlerFactory.Create<PrimaryGameEnded>().Handle(new PrimaryGameEnded(win));

                    _eventBus.Publish(new PrimaryGameEndedEvent(_gameId, _denom, _wagerCategory.Id, _gameHistory.CurrentLog, win));
                    break;
            }
        }

        private void HandleBonusEvents(Trigger trigger)
        {
            var bonusPayment = new PayBonus();

            _handlerFactory.Create<PayBonus>().Handle(bonusPayment);
            if (!bonusPayment.PendingPayment)
            {
                Fire(trigger, true);
            }
        }

        private bool Fire(Trigger trigger, bool verify = false)
        {
            if (verify && !_state.CanFire(trigger))
            {
                Logger.Warn($"Cannot transition with trigger: {trigger}");

                return false;
            }

            Logger.Debug($"Transitioning with trigger: {trigger}");

            _stateLock.EnterWriteLock();
            try
            {
                _state.Fire(trigger);
            }
            finally
            {
                _stateLock.ExitWriteLock();
            }

            return true;
        }

        private void FirePrimaryGameEscrow(long initialWager, byte[] data, IOutcomeRequest request)
        {
            Logger.Debug($"Transitioning with trigger: {_primaryGameEscrowTrigger.Trigger}");

            _stateLock.EnterWriteLock();
            try
            {
                if (_state.CanFire(_primaryGameEscrowTrigger.Trigger))
                {
                    _state.Fire(_primaryGameEscrowTrigger, initialWager, data, request);
                }
            }
            finally
            {
                _stateLock.ExitWriteLock();
            }
        }

        private void FirePrimaryGameStart(long initialWager, byte[] data)
        {
            Logger.Debug($"Transitioning with trigger: {_primaryGameStartTrigger.Trigger}");

            _stateLock.EnterWriteLock();
            try
            {
                if (_state.CanFire(_primaryGameStartTrigger.Trigger))
                {
                    _state.Fire(_primaryGameStartTrigger, initialWager, data);
                }
            }
            finally
            {
                _stateLock.ExitWriteLock();
            }
        }

        private void FireSecondaryGameStart(long stake)
        {
            Logger.Debug($"Transitioning with trigger: {_secondaryGameStartTrigger.Trigger}");

            _stateLock.EnterWriteLock();
            try
            {
                if (_state.CanFire(_secondaryGameStartTrigger.Trigger))
                {
                    _state.Fire(_secondaryGameStartTrigger, stake);
                }
            }
            finally
            {
                _stateLock.ExitWriteLock();
            }
        }

        private void FirePayResults(long finalWin)
        {
            Logger.Debug($"Transitioning with trigger: {_payResultTrigger.Trigger}");

            _stateLock.EnterWriteLock();
            try
            {
                _state.Fire(_payResultTrigger, finalWin);
            }
            finally
            {
                _stateLock.ExitWriteLock();
            }
        }

        private void SetGamePlayData()
        {
            _faulted = false;

            _gameId = _properties.GetValue(GamingConstants.SelectedGameId, 0);
            _denom = _properties.GetValue(GamingConstants.SelectedDenom, 0L);
            _wagerCategory = _properties.GetValue<IWagerCategory>(GamingConstants.SelectedWagerCategory, null);
        }

        private bool CheckGameRoundStartTimer()
        {
            if (!_gamePlayDuration.IsRunning)
            {
                return true;
            }

            var gameRoundMillis = _properties.GetValue(GamingConstants.GameRoundDurationMs,
                GamingConstants.DefaultMinimumGameRoundDurationMs);
            return _gamePlayDuration.Elapsed >= TimeSpan.FromMilliseconds(gameRoundMillis);
        }

        private enum Trigger
        {
            PlayInitiated,
            InitializationFailed,
            ProcessExited,
            PrimaryGameRequested,
            PrimaryGameRequestFailed,
            PrimaryGameStarted,
            WagerChanged,
            ProgressiveHit,
            SecondaryGameChoice,
            SecondaryGameRequested,
            SecondaryGameRequestFailed,
            SecondaryGameStarted,
            GameResult,
            GameEnded,
            GameIdle
        }
    }
}