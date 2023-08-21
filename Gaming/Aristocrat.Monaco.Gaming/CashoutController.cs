namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using System.Timers;
    using Contracts;
    using Hardware.Contracts.Button;
    using Hardware.Contracts.Printer;
    using Kernel;
    using Runtime;
    using Runtime.Client;

    public class CashoutController : IDisposable, ICashoutController, IService
    {
        private readonly IResponsibleGaming _responsibleGaming;
        private readonly IEventBus _eventBus;
        private readonly IGamePlayState _gamePlayState;
        private readonly IGameHistory _gameHistory;
        private readonly IRuntime _runtime;
        private const int ChimeRepeatFrequency = 2000;
        private bool _disposed;

        private Timer _soundTimer;

        public CashoutController(
            IResponsibleGaming respGaming,
            IEventBus eventBus,
            IGamePlayState gamePlayState,
            IGameHistory gameHistory,
            IRuntime runtime)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _gamePlayState = gamePlayState ?? throw new ArgumentNullException(nameof(gamePlayState));
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _responsibleGaming = respGaming ?? throw new ArgumentNullException(nameof(respGaming));
        }

        public bool PaperIsInChute { get; set; }

        public bool PaperInChuteNotificationActive { get; set; }

        public string Name => typeof(CashoutController).ToString();

        public ICollection<Type> ServiceTypes => new List<Type> { typeof(ICashoutController) };

        public void Initialize()
        {
            _responsibleGaming.ForceCashOut += OnForceCashOut;

            _eventBus.Subscribe<HardwareWarningEvent>(this, HandleEvent);
            _eventBus.Subscribe<HardwareWarningClearEvent>(this, HandleEvent);
            _eventBus.Subscribe<MissedStartupEvent>(this, HandleEvent);
            _eventBus.Subscribe<GameIdleEvent>(this, HandleEvent);
            _eventBus.Subscribe<PrimaryGameStartedEvent>(this, HandleEvent);
            _eventBus.Subscribe<SystemDisabledEvent>(this, HandleEvent);
            _eventBus.Subscribe<SystemEnabledEvent>(this, HandleEvent);
            _eventBus.Subscribe<SystemDownEvent>(this, HandleEvent);
        }

        public void GameRequestedCashout()
        {
            _runtime.UpdateFlag(RuntimeCondition.PendingHandpay, false);

            if (!PaperIsInChute)
            {
                ExecuteCashOut();
            }
            else
            {
                DisplayNotification();
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
                _responsibleGaming.ForceCashOut -= OnForceCashOut;
                if (_soundTimer != null)
                {
                    _soundTimer.Dispose();
                    _soundTimer = null;
                }
            }

            _disposed = true;
        }

        private void HandleEvent(MissedStartupEvent evt)
        {
            dynamic param = evt.MissedEvent;
            HandleEvent(param);
        }

        private void HandleEvent(HardwareWarningClearEvent evt)
        {
            if (evt.Warning != PrinterWarningTypes.PaperInChute)
            {
                return;
            }

            PaperIsInChute = false;
            ClearNotification();
        }

        private void HandleEvent(HardwareWarningEvent evt)
        {
            if (evt.Warning != PrinterWarningTypes.PaperInChute)
            {
                return;
            }

            PaperIsInChute = true;
            DisplayNotification();
        }

        private void OnForceCashOut(object sender, EventArgs e)
        {
            ExecuteCashOut();
        }

        private void ExecuteCashOut()
        {
            _eventBus.Publish(new CashOutButtonPressedEvent());
        }

        private void HandleEvent(IEvent evt)
        {
            //this method is required to support unhandled events.
        }

        private void HandleEvent(GameIdleEvent evt)
        {
            DisplayNotification();
        }

        private void HandleEvent(PrimaryGameStartedEvent evt)
        {
            ClearNotification();
        }

        private void HandleEvent(SystemDisabledEvent evt)
        {
            ClearNotification();
        }

        private void HandleEvent(SystemEnabledEvent evt)
        {
            DisplayNotification();
        }

        private void ClearNotification()
        {
            StopTimer();

            if (!PaperInChuteNotificationActive)
            {
                return;
            }

            PaperInChuteNotificationActive = false;
            _eventBus.Publish(new CashoutNotificationEvent(false));
            if ((_gamePlayState.Idle || _gamePlayState.InPresentationIdle) && _runtime.Connected)
            {
                _runtime.UpdateFlag(RuntimeCondition.AllowGameRound, true);
            }
        }

        private void DisplayNotification()
        {
            if (!PaperIsInChute || !_gamePlayState.Idle || _gameHistory.IsRecoveryNeeded)
            {
                return;
            }

            PaperInChuteNotificationActive = PaperIsInChute;

            _eventBus.Publish(new CashoutNotificationEvent(true));

            // Resend the event every configured period for sound.
            SetTimer();

        }

        private void HandleEvent(SystemDownEvent evt)
        {
            ClearNotification();
        }


        private void SetTimer()
        {
            StopTimer();

            _soundTimer = new Timer(ChimeRepeatFrequency);
            _soundTimer.Elapsed += OnTimedEvent;
            _soundTimer.AutoReset = true;
            _soundTimer.Enabled = true;
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            _eventBus.Publish(new CashoutNotificationEvent(true, true));
        }

        private void StopTimer()
        {
            if (_soundTimer == null)
            {
                return;
            }

            _soundTimer.Stop();
            _soundTimer.Elapsed -= OnTimedEvent;
            _soundTimer.Dispose();
            _soundTimer = null;
        }
    }
}