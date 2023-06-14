namespace Aristocrat.Monaco.Gaming.UI
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Aristocrat.Cabinet.Contracts;
    using Aristocrat.Monaco.Accounting.Contracts.HandCount;
    using Aristocrat.Monaco.Application.Contracts.Extensions;
    using Contracts;
    using ViewModels;
    using Views.Overlay;
    using Kernel;
    using MVVM;
    using log4net;
    using Aristocrat.Monaco.Accounting.Contracts;

    public class HandCountOverlayService : IHandCountOverlayService, IService, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
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
                ServiceManager.GetInstance().TryGetService<IPropertiesManager>()
            )
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
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    _timerDialogViewModel = new HandCountTimerDialogViewModel();
                    _timerDialog = new HandCountTimerDialog(_timerDialogViewModel);

                    _cashoutDialogViewModel = new CashoutDialogViewModel();
                    _cashoutDialog = new CashoutDialog(_cashoutDialogViewModel);
                });

            _eventBus.Subscribe<HandCountResetTimerElapsedEvent>(this, HandleEvent);
            _eventBus.Subscribe<HandCountResetTimerStartedEvent>(this, HandleEvent);
            _eventBus.Subscribe<HandCountResetTimerCancelledEvent>(this, HandleEvent);
            _eventBus.Subscribe<CashoutAmountPlayerConfirmationRequestedEvent>(this, Handle);
            _eventBus.Subscribe<CashoutAmountPlayerConfirmationReceivedEvent>(this, Handle);
            _eventBus.Subscribe<CashoutCancelledEvent>(this, Handle);
        }

        private void Handle(CashoutCancelledEvent evt)
        {
            _eventBus.Publish(new ViewInjectionEvent(_cashoutDialog, DisplayRole.Main, ViewInjectionEvent.ViewAction.Remove));
        }

        private void Handle(CashoutAmountPlayerConfirmationReceivedEvent evt)
        {
            _eventBus.Publish(new ViewInjectionEvent(_cashoutDialog, DisplayRole.Main, ViewInjectionEvent.ViewAction.Remove));
        }

        private void Handle(CashoutAmountPlayerConfirmationRequestedEvent evt)
        {
            _cashoutDialogViewModel.HandCountAmount = (long)(_handCountService.HandCount * _cashOutAmountPerHand).MillicentsToDollars();
            _eventBus.Publish(new ViewInjectionEvent(_cashoutDialog, DisplayRole.Main, ViewInjectionEvent.ViewAction.Add));
        }

        private void HandleEvent(HandCountResetTimerCancelledEvent obj)
        {
            HandCountResetTimerCancelled();
        }

        private void HandleEvent(HandCountResetTimerStartedEvent e)
        {
            HandCountResetTimerStarted();
        }

        private void HandleEvent(HandCountResetTimerElapsedEvent e)
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
    }
}
