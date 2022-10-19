namespace Aristocrat.Monaco.Gaming
{
    using Kernel;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Timers;
    using Accounting.Contracts;
    using Application.Contracts;
    using Contracts;
    using log4net;
    using Runtime;

    public class LobbyClockService : ILobbyClockService, IService, IDisposable
    {
        private const double GamePlayingIntervalInSeconds = 600_000d;
        private const double GameIdleIntervalInSeconds = 10000d;
        private const double NoCreditIntervalInSeconds = 30_000d;

        private IEventBus _eventBus;
        private IPropertiesManager _propertiesManager;
        private ISystemDisableManager _disableManager;
        private IBank _bank;
        private IGameProvider _gameProvider;
        private IRuntime _runtime;

        private readonly Timer _timeFlashingTimer;
        private readonly Timer _gameIdleTimer;
        private readonly Timer _noCreditTimer;

        private readonly decimal _denomMultiplier;

        private bool _isDisposed;
        private bool _flashingEnabled;

        public delegate void ShowClockEventHandler(object sender, bool shouldShow);
        public event ShowClockEventHandler Notify;

        public void SetFlashingEnabled(bool show) => OnNotify(show);

        private void OnNotify(bool shouldShow) =>
            Notify?.Invoke(this, shouldShow);

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
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
                SetFlashingEnabled(_flashingEnabled);
            }
        }

        public LobbyClockService(IEventBus eventBus,
                                 IPropertiesManager propertiesManager,
                                 ISystemDisableManager disableManager,
                                 IBank bank,
                                 IGameProvider gameProvider,
                                 IRuntime runtime)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _disableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));

            _denomMultiplier = (decimal)_propertiesManager.GetValue(ApplicationConstants.CurrencyMultiplierKey, 1d);

            _timeFlashingTimer = new Timer { Interval = GamePlayingIntervalInSeconds };
            _timeFlashingTimer.Elapsed += TimeFlashingTimer_Tick;

            _gameIdleTimer = new Timer { Interval = GameIdleIntervalInSeconds };
            _gameIdleTimer.Elapsed += GameIdleTimer_Tick;

            _noCreditTimer =  new Timer { Interval = NoCreditIntervalInSeconds };
            _noCreditTimer.Elapsed += NoCreditTimer_Tick;

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

        }

        private void TriggerTimeFlashing()
        {
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
            _gameIdleTimer.Start();
        }
    }
}