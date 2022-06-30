namespace Aristocrat.Monaco.Gaming.UI
{
    using System;
    using System.Reflection;
    using System.Threading;
    using Accounting.Contracts;
    using Contracts;
    using Contracts.Lobby;
    using Contracts.Models;
    using Hardware.Contracts.Bell;
    using Kernel;
    using Models;
    using log4net;
    using Stateless;

    /// <summary>
    ///     LobbyStateManager manages all state for the Lobby.
    ///     All State Transitions go through this class, triggered by
    ///     calls to SendTrigger in the ILobbyStateManager interface.
    /// </summary>
    public class LobbyStateManager : ILobbyStateManager
    {
        private const int AttractModeIdleTimeoutInSeconds = 30;
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IBank _bank;
        private readonly IBell _bell;
        private readonly IGameHistory _gameHistory;
        private readonly IPropertiesManager _properties;
        private LobbyConfiguration _config;
        private bool _disposed;
        private bool _ageWarningOnEnable;
        private StateMachine<LobbyState, LobbyTrigger> _state;
        private ReaderWriterLockSlim _stateLock;
        private LobbyCashOutState _cashOutState = LobbyCashOutState.Undefined;
        private readonly LobbyStateQueue _lobbyStateQueue;
        private DateTime? _lastUserInteraction;
        private StateMachine<LobbyState, LobbyTrigger>.TriggerWithParameters<bool> _initiateRecoveryTrigger;
        private StateMachine<LobbyState, LobbyTrigger>.TriggerWithParameters<GameInfo> _launchGameForReplayTrigger;
        private StateMachine<LobbyState, LobbyTrigger>.TriggerWithParameters<GameInfo> _launchGameTrigger;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LobbyStateManager" /> class.
        /// </summary>
        /// <param name="gameHistory">The game history service.</param>
        /// <param name="properties">The properties manager.</param>
        /// <param name="bell">The bell service.</param>
        /// <param name="bank">The bank service.</param>
        public LobbyStateManager(
            IGameHistory gameHistory,
            IPropertiesManager properties,
            IBell bell,
            IBank bank)
        {
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _bell = bell ?? throw new ArgumentNullException(nameof(bell));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));

            _stateLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            _lobbyStateQueue = new LobbyStateQueue();
            CreateStateMachine(LobbyState.Startup);

            AllowGameAutoLaunch = true;
        }

        private BannerDisplayMode _lobbyBannerDisplayMode = BannerDisplayMode.Blinking;

        /// <summary>
        ///     Gets the Current State of the Lobby
        /// </summary>
        public LobbyState CurrentState
        {
            get
            {
                _stateLock?.EnterReadLock();
                try
                {
                    return _state.State;
                }
                finally
                {
                    _stateLock?.ExitReadLock();
                }
            }
        }

        public LobbyCashOutState CashOutState
        {
            get => _cashOutState;
            set
            {
                if (_cashOutState == value)
                {
                    return;
                }

                _cashOutState = value;
                Logger.Debug($"CashOutState: {value}");
            }
        }

        public LobbyState PreviousState { get; private set; }

        public CashInType LastCashInType { get; set; }

        public bool UnexpectedGameExitWhileDisabled { get; set; }

        public bool IsLoadingGameForRecovery { get; set; }

        public bool AllowGameInCharge { get; set; }

        public bool AllowGameAutoLaunch { private get; set; }

        public bool IsSingleGame { private get; set; }

        public bool AllowSingleGameAutoLaunch => AllowGameInCharge && AllowGameAutoLaunch && IsSingleGame;

        public bool IsTabView { get; set; }

        public bool ResetAttractOnInterruption { get; set; }

        public Action<LobbyState, object> OnStateEntry { get; set; }

        public Action<LobbyState, object> OnStateExit { get; set; }

        public Action GameLoadedWhileDisabled { get; set; }

        public Action UpdateLobbyUI { get; set; }

        public Func<AgeWarningCheckResult> CheckForAgeWarning { get; set; }

        /// <summary>
        ///     Initialize must be called before using the LobbyStateManager
        /// </summary>
        public void Initialize()
        {
            _config = (LobbyConfiguration)_properties.GetProperty(GamingConstants.LobbyConfig, null);
            ConfigureStates();
            _lastUserInteraction = DateTime.UtcNow;
        }

        public void SendTrigger(LobbyTrigger trigger, object parameter = null)
        {
            if (_stateLock == null)
            {
                Logger.Warn("StateLock is NULL. Bailing from SendTrigger. We are probably shutting down.");
                return;
            }

            // Special case for Changing Idle Text Mode
            if (trigger == LobbyTrigger.SetLobbyIdleTextStatic || trigger == LobbyTrigger.SetLobbyIdleTextScrolling)
            {
                _lobbyBannerDisplayMode = trigger == LobbyTrigger.SetLobbyIdleTextStatic
                    ? BannerDisplayMode.Blinking
                    : BannerDisplayMode.Scrolling;

                if (!_state.CanFire(trigger))
                {
                    if (trigger == LobbyTrigger.SetLobbyIdleTextStatic &&
                       (BaseState == LobbyState.ChooserScrollingIdleText ||
                        BaseState == LobbyState.ChooserIdleTextTimer))
                    {
                        _lobbyStateQueue.SetNewBaseState(LobbyState.Chooser);
                        UpdateLobbyUI();
                    }
                    else if (trigger == LobbyTrigger.SetLobbyIdleTextScrolling && BaseState == LobbyState.Chooser)
                    {
                        _lobbyStateQueue.SetNewBaseState(LobbyState.ChooserScrollingIdleText);
                        UpdateLobbyUI();
                    }
                    return;
                }
            }

            if (!_state.CanFire(trigger))
            {
                Logger.Warn($"Cannot transition with trigger {trigger}. Current State [{CurrentState}]");
                return;
            }

            Logger.Debug($"Transitioning with trigger {trigger}");

            _stateLock?.EnterWriteLock();
            try
            {
                if (parameter == null)
                {
                    _state.Fire(trigger);
                }
                else
                {
                    if (trigger == LobbyTrigger.LaunchGame && parameter is GameInfo info)
                    {
                        _state.Fire(_launchGameTrigger, info);
                    }
                    else if (trigger == LobbyTrigger.LaunchGameForDiagnostics && parameter is GameInfo gameInfo)
                    {
                        _state.Fire(_launchGameForReplayTrigger, gameInfo);
                    }
                    else if (trigger == LobbyTrigger.InitiateRecovery && parameter is bool state)
                    {
                        _state.Fire(_initiateRecoveryTrigger, state);
                    }
                    else
                    {
                        throw new ArgumentException($"Unsupported Trigger Parameter Type {parameter}");
                    }
                }
            }
            finally
            {
                _stateLock?.ExitWriteLock();
            }
        }

        public bool IsInState(LobbyState state)
        {
            return _state.IsInState(state);
        }

        public void OnUserInteraction()
        {
            _lastUserInteraction = DateTime.UtcNow;
        }

        public bool IsStateInResponsibleGamingInfo(LobbyState state)
        {
            return state == LobbyState.ResponsibleGamingInfo ||
                   state == LobbyState.ResponsibleGamingInfoLayeredLobby ||
                   state == LobbyState.ResponsibleGamingInfoLayeredGame;
        }

        public LobbyState BaseState => _lobbyStateQueue.BaseState;

        public bool ContainsAnyState(params LobbyState[] states)
        {
            return _lobbyStateQueue.ContainsAny(states);
        }

        public void AddStackableState(LobbyState state)
        {
            _lobbyStateQueue.AddStackableState(state);
        }

        public void RemoveStackableState(LobbyState state)
        {
            _lobbyStateQueue.RemoveStackableState(state);
        }

        public void AddFlagState(LobbyState state, object param = null, object param2 = null)
        {
            Logger.Debug($"AddFlagState [{state}]");
            _lobbyStateQueue.AddFlagState(state);

            switch (state)
            {
                case LobbyState.CashIn:
                    if (param is CashInType cashInType)
                    {
                        CashInStarted(cashInType);
                    }
                    break;
                case LobbyState.CashOut:
                    CashOutStarted();
                    break;
            }
            UpdateLobbyUI();
        }

        public void RemoveFlagState(LobbyState state, object param = null)
        {
            Logger.Debug($"RemoveFlagState [{state}]");
            _lobbyStateQueue.RemoveFlagState(state);

            switch (state)
            {
                case LobbyState.CashIn:
                    CashInFinished();
                    break;
                case LobbyState.CashOut:
                    if (param is bool success)
                    {
                        CashOutFinished(success);
                    }
                    break;
            }
            UpdateLobbyUI();
        }

        /// <summary>
        ///     dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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
                _stateLock.Dispose();
                _lobbyStateQueue.Dispose();
            }

            _stateLock = null;

            _disposed = true;
        }

        private void CreateStateMachine(LobbyState initialState)
        {
            _stateLock?.EnterWriteLock();
            try
            {
                _state = new StateMachine<LobbyState, LobbyTrigger>(initialState);
            }
            finally
            {
                _stateLock?.ExitWriteLock();
            }
        }

        private void ConfigureStates()
        {
            _launchGameTrigger = _state.SetTriggerParameters<GameInfo>(LobbyTrigger.LaunchGame);
            _launchGameForReplayTrigger = _state.SetTriggerParameters<GameInfo>(LobbyTrigger.LaunchGameForDiagnostics);
            _initiateRecoveryTrigger = _state.SetTriggerParameters<bool>(LobbyTrigger.InitiateRecovery);

            _state.Configure(LobbyState.Startup)
                .PermitIf(LobbyTrigger.LobbyEnter, LobbyState.Chooser, () => _lobbyBannerDisplayMode == BannerDisplayMode.Blinking)
                .PermitIf(LobbyTrigger.LobbyEnter, LobbyState.ChooserScrollingIdleText, () => _lobbyBannerDisplayMode == BannerDisplayMode.Scrolling)
                .Permit(LobbyTrigger.InitiateRecovery, LobbyState.RecoveryFromStartup)
                .Permit(LobbyTrigger.LaunchGame, LobbyState.GameLoading)
                .OnExit(CallStateExit);

            _state.Configure(LobbyState.Chooser)
                .OnEntry(() => CallStateEntry(LobbyState.Chooser)) // for states with sub states we have to be explicit
                .PermitIf(LobbyTrigger.AttractTimer, LobbyState.Attract, () => CurrentState == LobbyState.Chooser)
                .PermitIf(LobbyTrigger.LaunchGame, LobbyState.GameLoading, () => !_gameHistory.IsRecoveryNeeded)
                .Permit(LobbyTrigger.ResponsibleGamingInfoButton, LobbyState.ResponsibleGamingInfo)
                .Permit(LobbyTrigger.Disable, LobbyState.Disabled)
                .Permit(LobbyTrigger.InitiateRecovery, LobbyState.Recovery)
                .Permit(LobbyTrigger.ResponsibleGamingTimeLimitDialog, LobbyState.ResponsibleGamingTimeLimitDialog)
                .PermitIf(LobbyTrigger.AgeWarningDialog, LobbyState.AgeWarningDialog, () => _config.DisplayAgeWarning)
                .PermitIf(LobbyTrigger.GameLoaded, LobbyState.Game, () => AllowSingleGameAutoLaunch)
                .Permit(LobbyTrigger.SetLobbyIdleTextScrolling, LobbyState.ChooserScrollingIdleText)
                .OnExit(() => CallStateExit(LobbyState.Chooser));

            _state.Configure(LobbyState.ChooserScrollingIdleText)
                .SubstateOf(LobbyState.Chooser)
                .OnEntry(CallStateEntry)
                .OnEntryFrom(LobbyTrigger.IdleTextTimer, () => UpdateLobbyUI())
                .PermitDynamic(
                    LobbyTrigger.IdleTextScrollingComplete,
                    () => _bank.QueryBalance() == 0 && IsAttractModeIdleTimeout()
                        ? LobbyState.Attract
                        : LobbyState.ChooserIdleTextTimer)
                .Permit(LobbyTrigger.SetLobbyIdleTextStatic, LobbyState.Chooser)
                .OnExit(CallStateExit);

            _state.Configure(LobbyState.ChooserIdleTextTimer)
                .SubstateOf(LobbyState.Chooser)
                .OnEntry(CallStateEntry)
                .OnEntryFrom(LobbyTrigger.IdleTextScrollingComplete, () => UpdateLobbyUI())
                .Permit(LobbyTrigger.IdleTextTimer, LobbyState.ChooserScrollingIdleText)
                .OnExit(CallStateExit);

            _state.Configure(LobbyState.Attract)
                .OnEntry(CallStateEntry)
                .PermitDynamic(LobbyTrigger.AttractVideoComplete, GetDefaultChooserState)
                .PermitDynamic(LobbyTrigger.AttractModeExit, GetDefaultChooserState)
                .Permit(LobbyTrigger.AgeWarningDialog, LobbyState.AgeWarningDialog)
                .Permit(LobbyTrigger.LaunchGame, LobbyState.GameLoading)
                .PermitIf(LobbyTrigger.Disable, LobbyState.Disabled, () => !AllowSingleGameAutoLaunch)
                .PermitIf(LobbyTrigger.Disable, LobbyState.Chooser, () => AllowSingleGameAutoLaunch)
                .Permit(LobbyTrigger.ResponsibleGamingTimeLimitDialog, LobbyState.ResponsibleGamingTimeLimitDialog)
                .Permit(LobbyTrigger.InitiateRecovery, LobbyState.Recovery)
                .PermitIf(LobbyTrigger.GameLoaded, LobbyState.Game, () => AllowSingleGameAutoLaunch)
                .OnExit(CallStateExit);

            _state.Configure(LobbyState.GameLoading)
                .OnEntryFrom(_launchGameTrigger, CallStateEntry)
                .Permit(LobbyTrigger.GameLoaded, LobbyState.Game)
                .PermitIf(LobbyTrigger.Disable, LobbyState.Disabled, () => AllowSingleGameAutoLaunch || IsLoadingGameForRecovery)
                .PermitDynamic(
                    LobbyTrigger.GameNormalExit,
                    () => IsLoadingGameForRecovery ? LobbyState.Recovery : GetDefaultChooserState())
                .PermitDynamic(
                    LobbyTrigger.GameUnexpectedExit,
                    () => IsLoadingGameForRecovery ? LobbyState.Recovery : GetDefaultChooserState())
                .Permit(LobbyTrigger.InitiateRecovery, LobbyState.Recovery);

            _state.Configure(LobbyState.GameLoadingForDiagnostics)
                .SubstateOf(LobbyState.Disabled)
                .OnEntryFrom(_launchGameForReplayTrigger, CallStateEntry)
                .Permit(LobbyTrigger.GameLoaded, LobbyState.GameDiagnostics)
                .Permit(LobbyTrigger.GameDiagnosticsExit, LobbyState.Disabled);

            _state.Configure(LobbyState.Game)
                .OnEntry(GameLoaded)
                .PermitDynamic(LobbyTrigger.GameNormalExit, GetDefaultChooserState)
                .PermitDynamic(LobbyTrigger.GameUnexpectedExit, GetDefaultChooserState)
                .Permit(LobbyTrigger.Disable, LobbyState.Disabled)
                .Permit(LobbyTrigger.InitiateRecovery, LobbyState.Recovery)
                .PermitIf(LobbyTrigger.AttractTimer, LobbyState.Attract, () => AllowSingleGameAutoLaunch && _bank.QueryBalance() == 0)
                .Permit(LobbyTrigger.ResponsibleGamingTimeLimitDialog, LobbyState.ResponsibleGamingTimeLimitDialog)
                .PermitIf(LobbyTrigger.ReturnToLobby, LobbyState.Chooser, () => !AllowSingleGameAutoLaunch);

            _state.Configure(LobbyState.GameDiagnostics)
                .SubstateOf(LobbyState.Disabled)
                .OnEntry(() => CallStateEntry(LobbyState.GameDiagnostics))
                .Permit(LobbyTrigger.GameDiagnosticsExit, LobbyState.Disabled)
                .OnExit(CallStateExit);

            _state.Configure(LobbyState.ResponsibleGamingInfo)
                .OnEntry(() => CallStateEntry(LobbyState.ResponsibleGamingInfo))
                .PermitDynamic(LobbyTrigger.ResponsibleGamingInfoExit, GetDefaultChooserState)
                .PermitDynamic(LobbyTrigger.ResponsibleGamingInfoTimeOut, GetDefaultChooserState)
                .Permit(LobbyTrigger.LaunchGame, LobbyState.GameLoading)
                .Permit(LobbyTrigger.Disable, LobbyState.Disabled)
                .Permit(LobbyTrigger.ResponsibleGamingTimeLimitDialog, LobbyState.ResponsibleGamingTimeLimitDialog)
                .Permit(LobbyTrigger.InitiateRecovery, LobbyState.Recovery)
                .PermitIf(LobbyTrigger.PrintHelpline, LobbyState.PrintHelpline, () => _config.ResponsibleGamingInfo.PrintHelpline)
                .OnExit(CallStateExit);

            _state.Configure(LobbyState.ResponsibleGamingInfoLayeredLobby)
                .SubstateOf(LobbyState.ResponsibleGamingInfo)
                .Permit(LobbyTrigger.ResponsibleGamingInfoExit, LobbyState.ResponsibleGamingTimeLimitDialog)
                .Permit(LobbyTrigger.ResponsibleGamingInfoTimeOut, LobbyState.ResponsibleGamingTimeLimitDialog)
                .PermitDynamic(
                    LobbyTrigger.ResponsibleGamingTimeLimitDialogDismissed,
                    () =>
                    {
                        ResponsibleGamingExited();
                        return _lobbyStateQueue.TopState;
                    });

            _state.Configure(LobbyState.ResponsibleGamingInfoLayeredGame)
                .SubstateOf(LobbyState.ResponsibleGamingInfo)
                .Permit(LobbyTrigger.ResponsibleGamingInfoExit, LobbyState.ResponsibleGamingTimeLimitDialog)
                .Permit(LobbyTrigger.ResponsibleGamingInfoTimeOut, LobbyState.ResponsibleGamingTimeLimitDialog)
                .PermitDynamic(
                    LobbyTrigger.ResponsibleGamingTimeLimitDialogDismissed,
                    () =>
                    {
                        ResponsibleGamingExited();
                        return _lobbyStateQueue.TopState;
                    });

            _state.Configure(LobbyState.Disabled)
                .OnEntry(CallStateEntry)
                .InternalTransitionIf(LobbyTrigger.GameLoaded, () => !AllowSingleGameAutoLaunch, GameLoadedWhileDisabled)
                .InternalTransition(LobbyTrigger.GameNormalExit, () => GameExited(false))
                .InternalTransition(LobbyTrigger.GameUnexpectedExit, () => GameExited(true))
                .InternalTransition(
                    LobbyTrigger.ResponsibleGamingTimeLimitDialogDismissed,
                    ResponsibleGamingExited)
                .InternalTransition(LobbyTrigger.PrintHelplineComplete, PrintHelplineFinishedWhileDisabled)
                .InternalTransition(LobbyTrigger.ResponsibleGamingTimeLimitDialog, ResponsibleGamingWhileDisabled)
                .PermitIf(LobbyTrigger.InitiateRecovery, LobbyState.Recovery, () => CurrentState == LobbyState.Disabled)
                .PermitIf(
                    LobbyTrigger.LaunchGameForDiagnostics,
                    LobbyState.GameLoadingForDiagnostics,
                    () => CurrentState == LobbyState.Disabled)
                .PermitIf(LobbyTrigger.LaunchGame, LobbyState.GameLoading, () => AllowSingleGameAutoLaunch)
                .PermitDynamicIf(
                    LobbyTrigger.Enable,
                    () => _ageWarningOnEnable ? LobbyState.AgeWarningDialog : GetTopStateExcluding(LobbyState.Disabled),
                    () => CurrentState == LobbyState.Disabled)
                .InternalTransition(LobbyTrigger.IdleTextScrollingComplete, IdleTextScrollingComplete)  // should not happen--in here to protect us from unexpected event
                .InternalTransition(LobbyTrigger.IdleTextTimer, IdleTextTimer) // should not happen--in here to protect us from unexpected event
                .OnExit(() => CallStateExit(LobbyState.Disabled));

            _state.Configure(LobbyState.Recovery) // for states with sub states CallStateEntry has to specify specific LobbyState
                .OnEntryFrom(_initiateRecoveryTrigger, RecoveryInitiated)
                .OnEntryFrom(LobbyTrigger.GameNormalExit, () => RecoveryInitiated(false)) // these game exits only happen during game load
                .OnEntryFrom(LobbyTrigger.GameUnexpectedExit, () => RecoveryInitiated(false)) // in which case we always want to recover without asking runtime for permission.
                .Permit(LobbyTrigger.LaunchGame, LobbyState.GameLoading)
                .PermitDynamic(LobbyTrigger.LobbyEnter, GetDefaultChooserState);

            _state.Configure(LobbyState.RecoveryFromStartup)
                .SubstateOf(LobbyState.Recovery)
                .OnEntry(CallStateEntry);

            _state.Configure(LobbyState.ResponsibleGamingTimeLimitDialog)
                .OnEntry(ResponsibleGamingTimeLimitDialogEntered)
                .Permit(LobbyTrigger.Disable, LobbyState.Disabled)
                .Permit(LobbyTrigger.InitiateRecovery, LobbyState.Recovery)
                .PermitDynamic(
                    LobbyTrigger.ResponsibleGamingTimeLimitDialogDismissed,
                    () => GetTopStateExcluding(LobbyState.ResponsibleGamingTimeLimitDialog))
                .PermitIf(
                    LobbyTrigger.ResponsibleGamingInfoButton,
                    LobbyState.ResponsibleGamingInfoLayeredLobby,
                    () => _config.ResponsibleGamingInfo.FullScreen && _lobbyStateQueue.BaseState != LobbyState.Game)
                .PermitIf(
                    LobbyTrigger.ResponsibleGamingInfoButton,
                    LobbyState.ResponsibleGamingInfoLayeredGame,
                    () => _config.ResponsibleGamingInfo.FullScreen && _lobbyStateQueue.BaseState == LobbyState.Game)
                .InternalTransition(
                    LobbyTrigger.IdleTextScrollingComplete,
                    IdleTextScrollingComplete)
                .InternalTransition(LobbyTrigger.IdleTextTimer, IdleTextTimer)
                .InternalTransition(LobbyTrigger.GameNormalExit, () => GameExited(false))
                .InternalTransition(LobbyTrigger.GameUnexpectedExit, () => GameExited(true));

            _state.Configure(LobbyState.AgeWarningDialog)
                .OnEntry(AgeWarningDialogEntry)
                .Permit(LobbyTrigger.Disable, LobbyState.Disabled)
                .PermitDynamic(LobbyTrigger.AgeWarningTimeout, () => GetTopStateExcluding(LobbyState.AgeWarningDialog))
                .InternalTransition(LobbyTrigger.IdleTextScrollingComplete, IdleTextScrollingComplete)
                .InternalTransition(LobbyTrigger.IdleTextTimer, IdleTextTimer)
                .OnExit(CallStateExit);

            _state.Configure(LobbyState.PrintHelpline)
                .OnEntry(CallStateEntry)
                .PermitDynamic(LobbyTrigger.PrintHelplineComplete, () => GetTopStateExcluding(LobbyState.PrintHelpline))
                .Permit(LobbyTrigger.Disable, LobbyState.Disabled);

            _state.OnUnhandledTrigger(
                (state, trigger) =>
                {
                    Logger.Error($"Invalid transition to State [{state}]. Trigger: {trigger}");
                });

            _state.OnTransitioned(
                transition =>
                {
                    _lobbyStateQueue.HandleStateTransition(transition.Source, transition.Destination);
                    if (transition.Source == LobbyState.Attract && LobbyStateQueue.IsStateStackable(transition.Destination))
                    {
                        _lobbyStateQueue.SetNewBaseState(GetDefaultChooserState()); // Everything cancels Attract Mode.
                    }
                    else if (transition.Source == LobbyState.Disabled &&
                             (transition.Destination == LobbyState.ResponsibleGamingTimeLimitDialog || transition.Destination == LobbyState.AgeWarningDialog))
                    {
                        // this is a one-off where we are leaving disabled but HandleStateTransition does not remove it
                        _lobbyStateQueue.RemoveStackableState(LobbyState.Disabled);
                    }
                    else if (transition.Source == LobbyState.GameLoadingForDiagnostics && transition.Destination == LobbyState.GameDiagnostics)
                    {
                        // this is a one-off for replay loading.  HandleStateTransition does not remove it
                        _lobbyStateQueue.RemoveStackableState(LobbyState.GameLoadingForDiagnostics);
                    }
                    else if ((transition.Source == LobbyState.GameLoadingForDiagnostics || transition.Source == LobbyState.GameDiagnostics) &&
                             transition.Destination == LobbyState.Disabled)
                    {
                        // Since Replay is part of disabled, we need to re-trigger the Disabled State Entry Code.
                        CallStateEntry(LobbyState.Disabled);
                    }
                    else if (((transition.Source == LobbyState.ChooserScrollingIdleText ||
                              transition.Source == LobbyState.ChooserIdleTextTimer) &&
                             transition.Destination == LobbyState.Chooser) ||
                            (transition.Source == LobbyState.Chooser && transition.Destination == LobbyState.ChooserScrollingIdleText))
                    {
                        // if we are changing idle text state, call state entry even though we aren't "officially" re-entering the chooser state
                        CallStateEntry(LobbyState.Chooser);
                    }
                    else if (transition.Source == LobbyState.Disabled &&
                             transition.Destination == LobbyState.GameLoading && AllowSingleGameAutoLaunch)
                    {
                        // During Single Game Auto-Launch we can get into a place where we jump from Disable directly to Game Loading and we need to special case it.
                        PreviousState = LobbyState.Chooser;
                        _lobbyStateQueue.SetNewBaseState(LobbyState.GameLoading);
                        _lobbyStateQueue.RemoveStackableState(LobbyState.Disabled);
                    }

                    if (transition.Destination == LobbyState.Chooser || transition.Destination == LobbyState.Disabled)
                    {
                        _bell.StopBell();
                    }

                    PreviousState = transition.Source;

                    Logger.Debug($"Transitioned from state [{transition.Source}] to [{transition.Destination}]. Trigger: {transition.Trigger}");
                });
        }

        private void CallStateEntry(object obj = null)
        {
            CallStateEntry(CurrentState, obj);
        }

        private void CallStateEntry(LobbyState state, object obj = null)
        {
            OnStateEntry(state, obj);
        }

        private void CallStateExit(object obj = null)
        {
            CallStateExit(CurrentState, obj);
        }

        private void CallStateExit(LobbyState state, object obj = null)
        {
            OnStateExit(state, obj);
        }

        private void ResponsibleGamingTimeLimitDialogEntered()
        {
            if (_lobbyStateQueue.ContainsAny(LobbyState.ResponsibleGamingInfo))
            {
                Logger.Debug("Exiting ResponsibleGamingInfo to show Responsible Gaming Dialog");
                _lobbyStateQueue.RemoveStackableState(LobbyState.ResponsibleGamingInfo);
            }

            OnStateEntry(LobbyState.ResponsibleGamingTimeLimitDialog, null);
        }

        private LobbyState GetDefaultChooserState()
        {
            return _lobbyBannerDisplayMode == BannerDisplayMode.Scrolling ? LobbyState.ChooserScrollingIdleText : LobbyState.Chooser;
        }

        private void IdleTextScrollingComplete()
        {
            Logger.Debug("Idle Text Scrolling Completed.");
            _lobbyStateQueue.HandleStateTransition(LobbyState.ChooserScrollingIdleText, LobbyState.ChooserIdleTextTimer);
            CallStateEntry(LobbyState.ChooserIdleTextTimer); //have to call to get timer to start
            UpdateLobbyUI();
        }

        private void IdleTextTimer()
        {
            Logger.Debug("Idle Text Timer Triggered.");
            _lobbyStateQueue.HandleStateTransition(LobbyState.ChooserIdleTextTimer, LobbyState.ChooserScrollingIdleText);
            UpdateLobbyUI();
        }

        private void GameExited(bool unexpected)
        {
            // we don't need to reset the base state on replay--this is for standard game exiting during disabled, not replay game exiting.
            if (CurrentState == LobbyState.GameDiagnostics || CurrentState == LobbyState.GameLoadingForDiagnostics)
                return;

            Logger.Debug($"Game Exited While Disabled. Unexpected: {unexpected}");
            _lobbyStateQueue.SetNewBaseState(GetDefaultChooserState());
            UnexpectedGameExitWhileDisabled = unexpected &&
                                              _lobbyStateQueue.ContainsAny(LobbyState.Disabled) &&
                                              _lobbyStateQueue.BaseState == LobbyState.Game;

            UpdateLobbyUI();
        }

        private void CashOutStarted()
        {
            Logger.Debug("CashOut Started.");
            CallStateEntry(LobbyState.CashOut);
        }

        private void CashOutFinished(bool success)
        {
            Logger.Debug($"CashOut Finished. Success: {success}");
            CallStateExit(LobbyState.CashOut);
        }

        private void CashInStarted(CashInType cashInType)
        {
            Logger.Debug($"CashIn Started. Type: {cashInType}");
            LastCashInType = cashInType;
            CallStateEntry(LobbyState.CashIn);
        }

        private void CashInFinished()
        {
            Logger.Debug("CashIn Finished");
            if (CheckForAgeWarning() == AgeWarningCheckResult.DisableDeferred)
            {
                _ageWarningOnEnable = true;
            }
            CallStateExit(LobbyState.CashIn);
        }

        private void ResponsibleGamingWhileDisabled()
        {
            Logger.Debug("Responsible Gaming Triggered While Disabled.");
            _lobbyStateQueue.AddStackableState(LobbyState.ResponsibleGamingTimeLimitDialog);
        }

        private void ResponsibleGamingExited()
        {
            Logger.Debug("Responsible Gaming Exited.");
            _lobbyStateQueue.RemoveStackableState(LobbyState.ResponsibleGamingTimeLimitDialog);
            _lobbyStateQueue.RemoveStackableState(LobbyState.ResponsibleGamingInfoLayeredLobby);
            _lobbyStateQueue.RemoveStackableState(LobbyState.ResponsibleGamingInfoLayeredGame);
            _lobbyStateQueue.RemoveStackableState(LobbyState.PrintHelpline);
            UpdateLobbyUI();
        }

        private void PrintHelplineFinishedWhileDisabled()
        {
            Logger.Debug("Printing Helpline Finished While Disabled.");
            _lobbyStateQueue.RemoveStackableState(LobbyState.PrintHelpline);
        }

        private bool IsAttractModeIdleTimeout()
        {
            return _lastUserInteraction.HasValue &&
                   _lastUserInteraction.Value.AddSeconds(AttractModeIdleTimeoutInSeconds) <= DateTime.UtcNow;
        }

        private void RecoveryInitiated(bool processExited)
        {
            IsLoadingGameForRecovery = true;
            CallStateEntry(LobbyState.Recovery, processExited);
        }

        private void GameLoaded()
        {
            IsLoadingGameForRecovery = false;
            CallStateEntry(LobbyState.Game);
        }

        private void AgeWarningDialogEntry()
        {
            _ageWarningOnEnable = false;
            CallStateEntry(LobbyState.AgeWarningDialog);
        }

        private LobbyState GetTopStateExcluding(LobbyState state)
        {
            return _lobbyStateQueue.GetTopStateExcluding(state);
        }
    }
}
