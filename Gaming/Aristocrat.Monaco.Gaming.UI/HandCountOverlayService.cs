namespace Aristocrat.Monaco.Gaming.UI
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using Aristocrat.Cabinet.Contracts;
    using Aristocrat.Monaco.Accounting.Contracts.HandCount;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts.Events;
    using Aristocrat.Monaco.Gaming.UI.ViewModels;
    using Aristocrat.Monaco.Gaming.UI.Views.Overlay;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.MVVM;

    public class HandCountOverlayService : IHandCountOverlayService, IService, IDisposable
    {
        private bool _disposed;
        private IEventBus _eventBus;
        private readonly IPropertiesManager _properties;

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

        public HandCountOverlayService() : this(ServiceManager.GetInstance().GetService<IEventBus>(),
            ServiceManager.GetInstance().TryGetService<IPropertiesManager>())
        { }

        public HandCountOverlayService(IEventBus eventBus, IPropertiesManager properties)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        public void Initialize()
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    _timerDialogViewModel = new HandCountTimerDialogViewModel();
                    _timerDialog = new HandCountTimerDialog(_timerDialogViewModel);
                });

            _eventBus.Subscribe<HandCountResetTimerStartedEvent>(this, HandleEvent);
            _eventBus.Subscribe<HandCountResetTimerCancelledEvent>(this, HandleEvent);
            _eventBus.Subscribe<HandCountResetTimerElapsedEvent>(this, HandleEvent);
        }

        private void HandleEvent(HandCountResetTimerElapsedEvent obj)
        {
            ButtonDeckFilter.FilterMode = ButtonDeckFilterMode.Normal;

            _eventBus.Publish(new ViewInjectionEvent(_timerDialog, DisplayRole.Main, ViewInjectionEvent.ViewAction.Remove));
        }

        private void HandleEvent(HandCountResetTimerCancelledEvent obj)
        {
            ButtonDeckFilter.FilterMode = ButtonDeckFilterMode.Normal;

            _timerDialogViewModel.OnHandCountTimerCancelled();
            _eventBus.Publish(new ViewInjectionEvent(_timerDialog, DisplayRole.Main, ViewInjectionEvent.ViewAction.Remove));
        }

        private void HandleEvent(HandCountResetTimerStartedEvent obj)
        {
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
