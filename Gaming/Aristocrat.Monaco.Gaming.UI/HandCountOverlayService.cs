namespace Aristocrat.Monaco.Gaming.UI
{
    using Accounting.Contracts;
    using Accounting.Contracts.HandCount;
    using Application.Contracts.Extensions;
    using Cabinet.Contracts;
    using Contracts;
    using Contracts.HandCount;
    using Kernel;
    using log4net;
    using MVVM;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using ViewModels;
    using Views.Overlay;

    public class HandCountOverlayService : IHandCountOverlayService, IService, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private bool _disposed;
        private readonly IEventBus _eventBus;
        private readonly IHandCountService _handCountService;

        private HandCountTimerDialog _timerDialog;
        private HandCountTimerDialogViewModel _timerDialogViewModel;
        private IButtonDeckFilter _buttonDeckFilter;

        private CashoutDialog _cashoutDialog;
        private CashoutDialogViewModel _cashoutDialogViewModel;
        private readonly long _cashOutAmountPerHand;

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

        public HandCountOverlayService()
            : this(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().TryGetService<IHandCountService>(),
                ServiceManager.GetInstance().TryGetService<IPropertiesManager>())
        {
        }

        public HandCountOverlayService(IEventBus eventBus, IHandCountService handCountService, IPropertiesManager properties)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _handCountService = handCountService ?? throw new ArgumentNullException(nameof(handCountService));
            _cashOutAmountPerHand = properties.GetValue(AccountingConstants.CashoutAmountPerHandCount, 0L);
        }

        public void Initialize()
        {
            if (!_handCountService.HandCountServiceEnabled)
            {
                return;
            }

            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    _timerDialogViewModel = new HandCountTimerDialogViewModel();
                    _timerDialog = new HandCountTimerDialog(_timerDialogViewModel);

                    _cashoutDialogViewModel = new CashoutDialogViewModel();
                    _cashoutDialog = new CashoutDialog(_cashoutDialogViewModel);
                });

            _eventBus.Subscribe<HandCountResetTimerElapsedEvent>(this, Handle);
            _eventBus.Subscribe<HandCountResetTimerStartedEvent>(this, Handle);
            _eventBus.Subscribe<HandCountResetTimerCancelledEvent>(this, Handle);
            _eventBus.Subscribe<CashoutAmountAuthorizationRequestedEvent>(this, Handle);
            _eventBus.Subscribe<CashoutAmountAuthorizationReceivedEvent>(this, Handle);
            _eventBus.Subscribe<CashoutAuthorizationCancelledEvent>(this, Handle);
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
                _timerDialogViewModel.Dispose();
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private void Handle(CashoutAuthorizationCancelledEvent evt)
        {
            _eventBus.Publish(new ViewInjectionEvent(_cashoutDialog, DisplayRole.Main, ViewInjectionEvent.ViewAction.Remove));
        }

        private void Handle(CashoutAmountAuthorizationReceivedEvent evt)
        {
            _eventBus.Publish(new ViewInjectionEvent(_cashoutDialog, DisplayRole.Main, ViewInjectionEvent.ViewAction.Remove));
        }

        private void Handle(CashoutAmountAuthorizationRequestedEvent evt)
        {
            _cashoutDialogViewModel.HandCountAmount = (long)(_handCountService.HandCount * _cashOutAmountPerHand).MillicentsToDollars();
            _eventBus.Publish(new ViewInjectionEvent(_cashoutDialog, DisplayRole.Main, ViewInjectionEvent.ViewAction.Add));
        }

        private void Handle(HandCountResetTimerCancelledEvent obj)
        {
            HandCountResetTimerCancelled();
        }

        private void Handle(HandCountResetTimerStartedEvent e)
        {
            HandCountResetTimerStarted();
        }

        private void Handle(HandCountResetTimerElapsedEvent e)
        {
            Logger.Debug("HandCountResetTimerElapsed");
            _handCountService.ResetHandCount(e.ResidualAmount);
            ButtonDeckFilter.FilterMode = ButtonDeckFilterMode.Normal;

            _eventBus.Publish(new ViewInjectionEvent(_timerDialog, DisplayRole.Main, ViewInjectionEvent.ViewAction.Remove));
        }

        private void HandCountResetTimerCancelled()
        {
            Logger.Debug("HandCountResetTimerCancelled");
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
    }
}
