namespace Aristocrat.Monaco.Gaming.UI
{
    using Aristocrat.Cabinet.Contracts;
    using Aristocrat.Monaco.Accounting.Contracts.HandCount;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts.Events;
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
        private CashoutResetHandCount _cashoutResetHandCountDialog;
        private CashoutResetHandCountViewModel _cashoutResetHandCountViewModel;

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
                    _cashoutResetHandCountViewModel = new CashoutResetHandCountViewModel();
                    _cashoutResetHandCountDialog = new CashoutResetHandCount(_cashoutResetHandCountViewModel);
                }
                );
            _eventBus.Subscribe<CashOutDialogVisibilityEvent>(this, Handle);
            _eventBus.Subscribe<CashoutResetHandCountVisibilityChangedEvent>(this, Handle);
        }

        private void Handle(CashoutResetHandCountVisibilityChangedEvent obj)
        {
            _eventBus.Publish(new ViewInjectionEvent(_cashoutResetHandCountDialog, DisplayRole.Main, ViewInjectionEvent.ViewAction.Remove));
        }

        private void Handle(CashOutDialogVisibilityEvent evt)
        {
            _eventBus.Publish(new ViewInjectionEvent(_cashoutResetHandCountDialog, DisplayRole.Main, ViewInjectionEvent.ViewAction.Add));
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
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }
    }
}
