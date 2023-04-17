namespace Aristocrat.Monaco.Gaming.UI
{
    using Aristocrat.Cabinet.Contracts;
    using Aristocrat.Monaco.Accounting.Contracts.HandCount;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Gaming.UI.ViewModels;
    using Aristocrat.Monaco.Gaming.UI.Views.Overlay;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.MVVM;
    using System;
    using System.Collections.Generic;

    public class HandCountOverlayService : IService, IDisposable
    {
        private bool _disposed;
        private IEventBus _eventBus;
        private HandCountTimerDialog _timerDialog;
        private HandCountTimerDialogViewModel _timerDialogViewModel;

        public string Name { get; } = "HandCountOverlayService";

        public ICollection<Type> ServiceTypes => new[] { typeof(IService) };

        public HandCountOverlayService() : this(ServiceManager.GetInstance().GetService<IEventBus>()) { }

        public HandCountOverlayService(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public void Initialize()
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    _timerDialogViewModel = new HandCountTimerDialogViewModel();
                    _timerDialog = new HandCountTimerDialog(_timerDialogViewModel);
                }
                );

            _eventBus.Subscribe<HandCountResetTimerStartedEvent>(this, HandleEvent);
            _eventBus.Subscribe<HandCountResetTimerCancelledEvent>(this, HandleEvent);
            _eventBus.Subscribe<HandCountResetTimerElapsedEvent>(this, HandleEvent);
        }

        private void HandleEvent(HandCountResetTimerElapsedEvent obj)
        {
            _eventBus.Publish(new ViewInjectionEvent(_timerDialog, DisplayRole.Main, ViewInjectionEvent.ViewAction.Remove));
        }

        private void HandleEvent(HandCountResetTimerCancelledEvent obj)
        {
            _timerDialogViewModel.OnHandCountTimerCancelled();
            _eventBus.Publish(new ViewInjectionEvent(_timerDialog, DisplayRole.Main, ViewInjectionEvent.ViewAction.Remove));
        }

        private void HandleEvent(HandCountResetTimerStartedEvent obj)
        {
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
