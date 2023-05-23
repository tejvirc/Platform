namespace Aristocrat.Monaco.Gaming.UI
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Timers;
    using Aristocrat.Cabinet.Contracts;
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Accounting.Contracts.HandCount;
    using Contracts;
    using ViewModels;
    using Views.Overlay;
    using Aristocrat.Monaco.Hardware.Contracts.Door;
    using Kernel;
    using Aristocrat.Monaco.Kernel.Contracts.Events;
    using MVVM;
    using Kernel.Contracts;
    using log4net;

    public class HandCountOverlayService : IHandCountOverlayService, IService, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private bool _disposed;
        private readonly IEventBus _eventBus;
        private readonly IPropertiesManager _properties;
        private readonly IHandCountService _handCountService;
        private IGamePlayState _gameState = null;

        private HandCountTimerDialog _timerDialog;
        private HandCountTimerDialogViewModel _timerDialogViewModel;
        private IButtonDeckFilter _buttonDeckFilter;
        private bool _startedWithRecovery = false;

        private bool _resetTimerIsRunning;

        private readonly Timer _initResetTimer;
        private const double ResetTimerIntervalInMs = 15000;

        public string Name { get; } = "HandCountOverlayService";

        public ICollection<Type> ServiceTypes => new[] { typeof(IService), typeof(IHandCountOverlayService) };

        public IButtonDeckFilter ButtonDeckFilter
        {
            get
            {
                return _buttonDeckFilter ??= ServiceManager.GetInstance()
                    .GetService<IContainerService>().Container.GetInstance<IButtonDeckFilter>();
            }
        }

        public HandCountOverlayService() : this(ServiceManager.GetInstance().GetService<IEventBus>(),
            ServiceManager.GetInstance().TryGetService<IPropertiesManager>(),
            ServiceManager.GetInstance().TryGetService<IHandCountService>()
            )
        { }

        public HandCountOverlayService(IEventBus eventBus, IPropertiesManager properties, IHandCountService handCountService)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _handCountService = handCountService ?? throw new ArgumentNullException(nameof(handCountService));

            _initResetTimer = new Timer(ResetTimerIntervalInMs);
            _initResetTimer.Elapsed += InitHandCountReset;
        }

        public void Initialize()
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    _timerDialogViewModel = new HandCountTimerDialogViewModel();
                    _timerDialog = new HandCountTimerDialog(_timerDialogViewModel);
                });

            _eventBus.Subscribe<InitializationCompletedEvent>(this, HandleEvent);
            _eventBus.Subscribe<HandCountResetTimerElapsedEvent>(this, HandleEvent);

            _eventBus.Subscribe<RecoveryStartedEvent>(this, x =>
            {
                _startedWithRecovery = true;
                SuspendResetHandcount();
            });
            _eventBus.Subscribe<TransferOutStartedEvent>(this, x =>
            {
                //when the transfer out is due to recovery, this event will come before InitializationCompletedEvent.
                //so we need to set the flag here to let our InitializationCompletedEvent handler knowing it's in recovery mode.
                //If this is normal restart, the side effect can be ignored.
                if (!_startedWithRecovery)
                {
                    _startedWithRecovery = true;
                    SuspendResetHandcount();
                }
            });

            _eventBus.Subscribe<OpenEvent>(this, x => SuspendResetHandcount());
            _eventBus.Subscribe<ClosedEvent>(this, x => CheckIfBelowResetThreshold());

            _eventBus.Subscribe<SystemDisabledEvent>(this, x => SuspendResetHandcount());
            _eventBus.Subscribe<SystemEnabledEvent>(this, x => CheckIfBelowResetThreshold());

            _eventBus.Subscribe<PrimaryGameStartedEvent>(this, x => SuspendResetHandcount());
            _eventBus.Subscribe<GameEndedEvent>(this, x => CheckIfBelowResetThreshold());

            //If bank balance is changed and new balance is above the min required credits,
            //suspend the reset hand count if underway.
            _eventBus.Subscribe<BankBalanceChangedEvent>(this, HandleEvent);
        }

        private void InitHandCountReset(object sender, ElapsedEventArgs e)
        {
            //If the game is not in idle state, stop the first timer and don't start the second timer
            if (!_gameState.Idle && !_gameState.InPresentationIdle)
            {
                SuspendResetHandcount();
                return;
            }

            //start the second timer
            if (!_resetTimerIsRunning)
            {
                _resetTimerIsRunning = true;
                HandCountResetTimerStarted();
            }
        }

        private void HandleEvent(BankBalanceChangedEvent obj)
        {
            //inserted money
            if (obj.NewBalance > obj.OldBalance)
            {
                SuspendResetHandcount();
                CheckIfBelowResetThreshold();
            }
        }

        private void HandleEvent(InitializationCompletedEvent obj)
        {
            //Don't check in Recovery scenarios
            if (!_startedWithRecovery)
            {
                CheckIfBelowResetThreshold();
            }
        }

        private void SuspendResetHandcount()
        {
            Logger.Debug("SuspendResetHandcount");

            _initResetTimer.Stop();

            if (_resetTimerIsRunning)
            {
                HandCountResetTimerCancelled();
                _resetTimerIsRunning = false;
            }
        }

        public void CheckIfBelowResetThreshold()
        {
            Logger.Debug("CheckIfBelowResetThreshold");

            var balance = (long)_properties.GetProperty(PropertyKey.CurrentBalance, 0L);
            var minimumRequiredCredits = (long)_properties.GetProperty(AccountingConstants.HandCountMinimumRequiredCredits,
                AccountingConstants.HandCountDefaultRequiredCredits);

            _gameState ??= ServiceManager.GetInstance().GetService<IGamePlayState>();

            if ((_gameState.Idle || _gameState.InPresentationIdle)
                && balance < minimumRequiredCredits
                && (balance > 0 || _handCountService.HandCount > 0))
            {
                Logger.Debug($"CheckIfBelowResetThreshold: balance={balance} HandCount={_handCountService.HandCount} start init reset timer");
                _initResetTimer.Start();
            }
        }

        private void HandleEvent(HandCountResetTimerElapsedEvent e)
        {
            _resetTimerIsRunning = false;
            _handCountService.ResetHandCount(e.ResidualAmount);

            ButtonDeckFilter.FilterMode = ButtonDeckFilterMode.Normal;

            _eventBus.Publish(new ViewInjectionEvent(_timerDialog, DisplayRole.Main, ViewInjectionEvent.ViewAction.Remove));
        }

        private void HandCountResetTimerCancelled()
        {
            ButtonDeckFilter.FilterMode = ButtonDeckFilterMode.Normal;

            _timerDialogViewModel.OnHandCountTimerCancelled();
            _eventBus.Publish(new ViewInjectionEvent(_timerDialog, DisplayRole.Main, ViewInjectionEvent.ViewAction.Remove));
        }
        private void HandCountResetTimerStarted()
        {
            Logger.Debug("HandCountResetTimerStarted");
            ButtonDeckFilter.FilterMode = ButtonDeckFilterMode.Lockup;
            _eventBus.Publish(new ViewInjectionEvent(_timerDialog, DisplayRole.Main, ViewInjectionEvent.ViewAction.Add));
            _timerDialogViewModel.OnHandCountTimerStarted();
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
                SuspendResetHandcount();
                _initResetTimer.Dispose();
                _timerDialogViewModel.Dispose();
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }
    }
}
