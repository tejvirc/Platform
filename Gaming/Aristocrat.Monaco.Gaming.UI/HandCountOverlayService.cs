namespace Aristocrat.Monaco.Gaming.UI
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Aristocrat.Cabinet.Contracts;
    using Aristocrat.Monaco.Accounting.Contracts.HandCount;
    using Contracts;
    using ViewModels;
    using Views.Overlay;
    using Kernel;
    using MVVM;
    using log4net;

    public class HandCountOverlayService : IHandCountOverlayService, IService, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private bool _disposed;
        private readonly IEventBus _eventBus;
        private readonly IHandCountService _handCountService;

        private HandCountTimerDialog _timerDialog;
        private HandCountTimerDialogViewModel _timerDialogViewModel;
        private IButtonDeckFilter _buttonDeckFilter;

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
                ServiceManager.GetInstance().TryGetService<IHandCountService>()
            )
        {

        }

        public HandCountOverlayService(IEventBus eventBus, IHandCountService handCountService)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _handCountService = handCountService ?? throw new ArgumentNullException(nameof(handCountService));
        }

        public void Initialize()
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    _timerDialogViewModel = new HandCountTimerDialogViewModel();
                    _timerDialog = new HandCountTimerDialog(_timerDialogViewModel);
                });

            _eventBus.Subscribe<HandCountResetTimerElapsedEvent>(this, HandleEvent);
            _eventBus.Subscribe<HandCountResetTimerStartedEvent>(this, HandleEvent);
            _eventBus.Subscribe<HandCountResetTimerCancelledEvent>(this, HandleEvent);
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
