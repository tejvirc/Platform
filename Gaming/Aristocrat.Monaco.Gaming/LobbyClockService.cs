namespace Aristocrat.Monaco.Gaming
{
    using Kernel;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
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
            int i = 0;
            while (i < 10)
            {
                Thread.Sleep(1000);
                PeriodicAsync();
                i++;
            }
        }

        public void PeriodicAsync()
        {

            if (_lobbyStateManager.CurrentState == LobbyState.Game)
            {
                Logger.Debug($"Send Flash to Game: {DateTime.Now.ToString("hh:mm:ss tt")}");
                //await delayTask;
                //Thread.Sleep(1000);
            }

            if (FlashingEnabled)
            {
                FlashingEnabled = false;
                return;
            }
            FlashingEnabled = true;
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