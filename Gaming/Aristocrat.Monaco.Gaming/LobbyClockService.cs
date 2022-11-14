namespace Aristocrat.Monaco.Gaming
{
    using Kernel;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Timers;
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Common;
    using Contracts;
    using Contracts.Events;
    using Contracts.Lobby;
    using Contracts.Models;
    using log4net;
    using Runtime;
    using Stateless;

    public enum FlashStates
    {
        Idle,
        InSession,
        Flashing
    }

    public enum FlashStateTriggers
    {
        SessionStarted,
        InsufficientCredits,
        IdleElapsed,
        ResumeSession,
        StartFlashing,
        End
    }

    /// <summary>
    /// This class is responsible for flashing the Lobby clock and sending commands to runtime to flash the clock in the game.
    /// </summary>
    public class LobbyClockService : IService, IDisposable
    {

        private const int AmountOfFlashesLeft = 5;
        private const long TimeBetweenFlashesInMilliseconds = 1500;
        private const long CheckStateInMilliseconds = 100;

        // Time interval in Milliseconds
        private const long GamePlayingIntervalInMilliseconds = 20_000; // 10 minutes
        private const long GameIdleIntervalInMilliseconds = 25_000;
        private const long NoCreditIntervalInMilliseconds = 30_000;

        private int _flashesDone;
        private readonly IEventBus _eventBus;
        private readonly IPropertiesManager _propertiesManager;
        private readonly ISystemDisableManager _disableManager;
        private readonly IBank _bank;
        private readonly IGameProvider _gameProvider;
        private readonly IRuntime _runtime;
        private readonly ILobbyStateManager _lobbyStateManager;
        private readonly StateMachine<FlashStates, FlashStateTriggers> _state;

        private ISystemTimerWrapper _lobbyClockFlashTimer;
        private IStopwatch _sessionFlashesCountdown; // Every 10m
        private IStopwatch _timeSinceLastGameCountdown; // 60 seconds since last game
        private IStopwatch _insufficientCreditCountdown; // 30 seconds
        private IStopwatch _timeBetweenFlashesCountdown; // 1.5 seconds between flashes

        private readonly decimal _denomMultiplier;

        private bool _gameLoaded;
        private bool _isDisposed;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <inheritdoc />
        public string Name => GetType().Name;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(LobbyClockService) };

        public LobbyClockService(
            IEventBus eventBus,
            IPropertiesManager propertiesManager,
            ISystemDisableManager disableManager,
            IBank bank,
            IGameProvider gameProvider,
            ILobbyStateManager lobbyStateManager,
            IRuntime runtime)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _disableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _lobbyStateManager = lobbyStateManager ?? throw new ArgumentNullException(nameof(lobbyStateManager));

            _denomMultiplier = (decimal)_propertiesManager.GetValue(ApplicationConstants.CurrencyMultiplierKey, 1d);

            SetupTimers();

            _state = ConfigureStateMachine();
        }

        public void SetupTimers(ISystemTimerWrapper mainTimer = null,
                                IStopwatch sessionFlashesCountdown = null,
                                IStopwatch timeSinceLastGameCountdown = null,
                                IStopwatch insufficientCreditCountdown = null,
                                IStopwatch timeBetweenFlashesCountdown = null)
        {
            _lobbyClockFlashTimer = mainTimer ?? new SystemTimerWrapper(CheckStateInMilliseconds);

            _sessionFlashesCountdown = sessionFlashesCountdown ?? new StopwatchAdapter();
            _timeSinceLastGameCountdown = timeSinceLastGameCountdown ?? new StopwatchAdapter();
            _insufficientCreditCountdown = insufficientCreditCountdown ?? new StopwatchAdapter();
            _timeBetweenFlashesCountdown = timeBetweenFlashesCountdown ?? new StopwatchAdapter();
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _eventBus.UnsubscribeAll(this);
            _lobbyClockFlashTimer.Stop();
            _lobbyClockFlashTimer.Elapsed -= LobbyFlashCheckState;

            _isDisposed = true;
        }

        public void Initialize()
        {
            _eventBus.Subscribe<PrimaryGameStartedEvent>(this, HandleEvent);
            _eventBus.Subscribe<GameInitializationCompletedEvent>(this, HandleEvent);
            _eventBus.Subscribe<TerminateGameProcessEvent>(this, HandleEvent);
            _eventBus.Subscribe<GameExitedNormalEvent>(this, HandleEvent);
            _eventBus.Subscribe<GameEndedEvent>(this, HandleEvent);
            _eventBus.Subscribe<BankBalanceChangedEvent>(this, HandleEvent);
            _eventBus.Subscribe<CashOutButtonPressedEvent>(this, HandleEvent);

            _lobbyClockFlashTimer.Elapsed += LobbyFlashCheckState;
            _lobbyClockFlashTimer.Start();
        }

        // reset Stops and clears
        // Restart clears and starts again
        private void OnSessionIdle()
        {
            _timeSinceLastGameCountdown.Reset();
            _insufficientCreditCountdown.Reset();
            _timeBetweenFlashesCountdown.Reset();
            _sessionFlashesCountdown.Reset();
        }

        private void OnSessionStarted()
        {
            // Start the StopWatch
            if (!_sessionFlashesCountdown.IsRunning)
            {
                _timeSinceLastGameCountdown.Restart();
                _sessionFlashesCountdown.Start();
                _timeBetweenFlashesCountdown.Start();
                _state.Fire(FlashStateTriggers.StartFlashing);
            }
        }

        private void OnFlashingStarted()
        {
            // Flash Here
            _flashesDone = 0;
            _sessionFlashesCountdown.Restart();
            Flash();
        }

        private void Flash()
        {
            // Have a look
            if (_timeBetweenFlashesCountdown.ElapsedMilliseconds < TimeBetweenFlashesInMilliseconds)
            {
                return;
            }

            if (_lobbyStateManager.IsInState(LobbyState.Game) && _gameLoaded)
            {
                _runtime.OnSessionTickFlashClock();
                _timeBetweenFlashesCountdown.Restart();
                _flashesDone++;
                return;
            }

            if (_lobbyStateManager.IsInState(LobbyState.Chooser) && !_gameLoaded)
            {
                _eventBus.Publish(new LobbyClockFlashChangedEvent());
                _timeBetweenFlashesCountdown.Restart();
                _flashesDone++;
            }
        }

        public void LobbyFlashCheckState(object o, ElapsedEventArgs e)
        {
            if ((_state.IsInState(FlashStates.InSession) || _state.IsInState(FlashStates.Flashing)) &&
                _disableManager.IsDisabled)
            {
                EndSession();
                return;
            }

            if (_state.IsInState(FlashStates.Flashing) &&
                _timeBetweenFlashesCountdown.ElapsedMilliseconds >= TimeBetweenFlashesInMilliseconds)
            {
                if (_flashesDone < AmountOfFlashesLeft)
                {
                    Flash();
                    return;
                }

                _state.Fire(FlashStateTriggers.ResumeSession);
                return;

            }

            if (_state.IsInState(FlashStates.InSession) &&
                (_timeSinceLastGameCountdown.ElapsedMilliseconds >= GameIdleIntervalInMilliseconds ||
                 _insufficientCreditCountdown.ElapsedMilliseconds >= NoCreditIntervalInMilliseconds))
            {
                _state.Fire(FlashStateTriggers.IdleElapsed);
                return;
            }

            // Check against time
            if (_state.IsInState(FlashStates.InSession) &&
                _sessionFlashesCountdown.ElapsedMilliseconds >= GamePlayingIntervalInMilliseconds)
            {
                _state.Fire(FlashStateTriggers.StartFlashing);
            }
        }

        private void HandleEvent(PrimaryGameStartedEvent evt)
        {
            if (_state.IsInState(FlashStates.Idle))
            {
                _state.Fire(FlashStateTriggers.SessionStarted);
            }
        }

        private void HandleEvent(GameEndedEvent evt)
        {
            CheckCredit();
        }

        private void HandleEvent(BankBalanceChangedEvent evt)
        {
            if (_state.IsInState(FlashStates.Idle))
            {
                return;
            }
            CheckCredit();
        }

        private void CheckCredit()
        {
            if (IsCreditSufficient())
            {
                StartIdleTimer();
            }
            else
            {
                StartNoCreditTimer();
            }
        }

        private void StartIdleTimer()
        {
            _insufficientCreditCountdown.Reset();
            _timeSinceLastGameCountdown.Restart();
        }

        private void StartNoCreditTimer()
        {
            _timeSinceLastGameCountdown.Reset();
            _insufficientCreditCountdown.Restart();
        }

        private void HandleEvent(GameInitializationCompletedEvent evt)
        {
            _timeBetweenFlashesCountdown.Restart();
            _gameLoaded = true;
            AddExtraFlash();
        }

        private void HandleEvent(TerminateGameProcessEvent evt)
        {
            _timeBetweenFlashesCountdown.Restart();
            _gameLoaded = false;
            AddExtraFlash();
        }

        private void HandleEvent(GameExitedNormalEvent evt)
        {
            _timeBetweenFlashesCountdown.Restart();
            _gameLoaded = false;
            AddExtraFlash();
        }

        private void HandleEvent(CashOutButtonPressedEvent evt)
        {
            EndSession();
            _insufficientCreditCountdown.Reset();
        }

        private void EndSession()
        {
            _sessionFlashesCountdown.Stop();
            _insufficientCreditCountdown.Stop();
            _timeSinceLastGameCountdown.Stop();
            _flashesDone = 0;
            _state.Fire(FlashStateTriggers.End);
        }

        // For the Corner case where the lobby or game is exited right at the time a new
        // flash is started. Here if we change state, there might be an extra flash.
        // Ensures flashes >= 5
        private void AddExtraFlash()
        {
            if (_state.IsInState(FlashStates.Flashing))
            {
                _flashesDone -= 1;
            }
        }

        private bool IsCreditSufficient()
        {
            var game = _gameProvider.GetGame(_propertiesManager.GetValue(GamingConstants.SelectedGameId, 0));

            if (game == null)
            {
                Logger.Debug($"Selected Game does not exist. Selected Game ID: {GamingConstants.SelectedGameId}");
                return false;
            }

            var minActiveDenom = game.ActiveDenominations.Min() / _denomMultiplier;

            var balanceInDollar = _bank.QueryBalance().MillicentsToDollars();

            return balanceInDollar >= game.MinimumWagerCredits * minActiveDenom;
        }

        private StateMachine<FlashStates, FlashStateTriggers> ConfigureStateMachine()
        {
            var stateMachine =
                new StateMachine<FlashStates, FlashStateTriggers>(
                    FlashStates.Idle);

            stateMachine.Configure(FlashStates.Idle)
                .OnEntry(OnSessionIdle)
                .Permit(FlashStateTriggers.SessionStarted, FlashStates.InSession);

            stateMachine.Configure(FlashStates.InSession)
                .OnEntry(OnSessionStarted)
                .Permit(FlashStateTriggers.InsufficientCredits, FlashStates.Idle)
                .Permit(FlashStateTriggers.IdleElapsed, FlashStates.Idle)
                .Permit(FlashStateTriggers.End, FlashStates.Idle)
                .Permit(FlashStateTriggers.StartFlashing, FlashStates.Flashing);

            stateMachine.Configure(FlashStates.Flashing)
                .OnEntry(OnFlashingStarted)
                .Permit(FlashStateTriggers.ResumeSession, FlashStates.InSession)
                .Permit(FlashStateTriggers.End, FlashStates.Idle);

            stateMachine.OnUnhandledTrigger(
                (state, trigger) =>
                {
                    Logger.Error($"Invalid State Transition. State : {state} Trigger : {trigger}");
                });

            stateMachine.OnTransitioned(
                transition =>
                {
                    Logger.Debug(
                        $"Transitioned From : {transition.Source} To : {transition.Destination} Trigger : {transition.Trigger}");
                });

            return stateMachine;
        }
    }
}