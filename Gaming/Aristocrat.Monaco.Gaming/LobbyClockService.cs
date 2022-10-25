namespace Aristocrat.Monaco.Gaming
{
    using Kernel;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Contracts;
    using Contracts.Lobby;
    using Contracts.Models;
    using log4net;
    using Runtime;
    using Timer = System.Timers.Timer;

    public class LobbyClockService : ILobbyClockService, IService, IDisposable
    {

        private const int NumberOfFlashes = 5;
        private const int TimeBetweenFlashes = 1000;

        // Time interval in Milliseconds
        private const double GamePlayingIntervalInMilliseconds = 600_000d;
        private const double GameIdleIntervalInMilliseconds = 25_000d;
        private const double NoCreditIntervalInMilliseconds = 30_000d;

        private IEventBus _eventBus;
        private IPropertiesManager _propertiesManager;
        private ISystemDisableManager _disableManager;
        private IBank _bank;
        private IGameProvider _gameProvider;
        private IRuntime _runtime;
        private ILobbyStateManager _lobbyStateManager;

        private readonly Timer _timeFlashingTimer;
        private readonly Timer _gameIdleTimer;
        private readonly Timer _noCreditTimer;

        private readonly decimal _denomMultiplier;

        private bool _isDisposed;
        private bool _flashingEnabled;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public event EventHandler<bool> Notify;

        /// <inheritdoc />
        public string Name => GetType().Name;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(ILobbyClockService) };

        public bool FlashingEnabled
        {
            get => _flashingEnabled;
            set
            {
                _flashingEnabled = (value && !_disableManager.IsDisabled);
                Notify?.Invoke(this, _flashingEnabled);
            }
        }

        public LobbyClockService(IEventBus eventBus,
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

            _timeFlashingTimer = new Timer { Interval = GamePlayingIntervalInMilliseconds };
            _timeFlashingTimer.Elapsed += TimeFlashingTimer_Tick;

            _gameIdleTimer = new Timer { Interval = GameIdleIntervalInMilliseconds };
            _gameIdleTimer.Elapsed += GameIdleTimer_Tick;

            _noCreditTimer =  new Timer { Interval = NoCreditIntervalInMilliseconds };
            Logger.Debug($"Class Thread: - {Thread.CurrentThread.ManagedThreadId}");
            //_noCreditTimer.Elapsed += NoCreditTimer_Tick;

        }

        private void NoCreditTimer_Tick(object sender, EventArgs e)
        {
            StopFlashing();
        }

        private void TimeFlashingTimer_Tick(object sender, EventArgs e)
        {
            TriggerTimeFlashing();
        }

        private void GameIdleTimer_Tick(object sender, EventArgs e)
        {
            //StopFlashing();
            TriggerTimeFlashing();
        }

        private void StopFlashing()
        {
            FlashingEnabled = false;
            _timeFlashingTimer.Stop();
            _gameIdleTimer.Stop();
        }

        private void TriggerTimeFlashing()
        {
            int flashIndex = 0;
            while (_lobbyStateManager.CurrentState == LobbyState.GameLoading)
            {
                Thread.Sleep(TimeBetweenFlashes);
            }

            if (RuntimeStateIsOkToFlash())
            {
                while (flashIndex < NumberOfFlashes)
                {
                    // Check we have not changed state
                    if (LobbyStateIsOkToFlash())
                    {
                        flashIndex = PlatformFlash(flashIndex);
                        continue;
                    }

                    flashIndex = RuntimeFlash(flashIndex);
                }
            }

            if (LobbyStateIsOkToFlash())
            {
                while (flashIndex < NumberOfFlashes)
                {
                    // Check the state has not changed
                    if (RuntimeStateIsOkToFlash())
                    {
                        flashIndex = RuntimeFlash(flashIndex);
                        continue;

                    }

                    flashIndex = PlatformFlash(flashIndex);
                }
            }
        }

        private int RuntimeFlash(int numberOfFlashesComplete)
        {
            Thread.Sleep(TimeBetweenFlashes);
            // Have we changed states since, waiting?
            if (RuntimeStateIsOkToFlash())
            {
                _runtime.OnSessionTickFlashClock();
                Logger.Debug($"Send Flash to Game: {DateTime.Now.ToString("hh:mm:ss tt")} - {numberOfFlashesComplete} - ThreadId {Thread.CurrentThread.ManagedThreadId}");
                return numberOfFlashesComplete + 1;
            }

            return numberOfFlashesComplete;
        }

        private int PlatformFlash(int numberOfFlashesComplete)
        {
            Thread.Sleep(TimeBetweenFlashes/2);

            if (FlashingEnabled)
            {
                FlashingEnabled = false;
                return numberOfFlashesComplete;
            }

            // Have we changed states since, waiting?
            if (LobbyStateIsOkToFlash())
            { 
                Logger.Debug($"Send Flash to Platform: {DateTime.Now.ToString("hh:mm:ss tt")} - {numberOfFlashesComplete}- ThreadId {Thread.CurrentThread.ManagedThreadId} - LobbyState - {_lobbyStateManager.CurrentState.ToString()}");
                FlashingEnabled = true;
                return numberOfFlashesComplete + 1;
            }

            return numberOfFlashesComplete;
        }

        private bool LobbyStateIsOkToFlash()
        {
            if (_lobbyStateManager.CurrentState != LobbyState.Game &&
                _lobbyStateManager.CurrentState != LobbyState.GameLoading &&
                _lobbyStateManager.CurrentState != LobbyState.GameDiagnostics)
            {
                return true;
            }
            return false;
        }

        private bool RuntimeStateIsOkToFlash()
        {
            if (_lobbyStateManager.CurrentState == LobbyState.Game &&
                _lobbyStateManager.CurrentState != LobbyState.GameLoading &&
                _lobbyStateManager.CurrentState != LobbyState.GameDiagnostics &&
                _lobbyStateManager.CurrentState != LobbyState.Chooser)
            {
                return true;
            }
            return false;
        }
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _eventBus.UnsubscribeAll(this);

            _timeFlashingTimer.Elapsed -= TimeFlashingTimer_Tick;
            _gameIdleTimer.Elapsed -= GameIdleTimer_Tick;
            _noCreditTimer.Elapsed -= NoCreditTimer_Tick;

            _timeFlashingTimer.Stop();
            _gameIdleTimer.Stop();
            _noCreditTimer.Stop();

            _isDisposed = true;
        }

        public void Initialize()
        {
            _eventBus.Subscribe<PrimaryGameStartedEvent>(this, HandleEvent);
            _eventBus.Subscribe<GameEndedEvent>(this, HandleEvent);
            _eventBus.Subscribe<BankBalanceChangedEvent>(this, HandleEvent);
            _eventBus.Subscribe<CashOutButtonPressedEvent>(this, HandleEvent);
            _gameIdleTimer.Start();
        }

        private void HandleEvent(PrimaryGameStartedEvent evt)
        {
            _gameIdleTimer.Stop();

            if (_timeFlashingTimer.Enabled)
            {
                return;
            }

            TriggerTimeFlashing();

            _timeFlashingTimer.Start();
        }

        private void HandleEvent(GameEndedEvent evt)
        {
            CheckCredit();
        }

        private void HandleEvent(BankBalanceChangedEvent evt)
        {
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
            _noCreditTimer.Stop();

            if (!_gameIdleTimer.Enabled)
            {
                _gameIdleTimer.Start();
            }
        }

        private void StartNoCreditTimer()
        {
            _gameIdleTimer.Stop();

            if (!_noCreditTimer.Enabled)
            {
                _noCreditTimer.Start();
            }
        }

        private void HandleEvent(CashOutButtonPressedEvent evt)
        {
            FlashingEnabled = false;
            _noCreditTimer.Stop();
            _gameIdleTimer.Stop();
            _timeFlashingTimer.Stop();
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
    }
}