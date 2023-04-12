namespace Aristocrat.Monaco.Gaming.UI.ViewModels
{
    using MVVM.Command;
    using System;
    using System.Windows.Input;
    using Aristocrat.MVVM.Model;
    using Aristocrat.MVVM.ViewModel;
    using Aristocrat.Monaco.Gaming.Contracts.Events;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Accounting.Contracts.HandCount;
    using Aristocrat.Monaco.Gaming.Contracts;

    public class CashoutResetHandCountViewModel : BaseEntityViewModel
    {
        private readonly IEventBus _eventBus;
        private bool _showDialog;

        public ICommand CashoutResetDialogYesNoCommand { get; }

        public bool ShowDialog
        {
            get
            {
                return _showDialog;
            }
            set
            {
                _showDialog = value;
                _eventBus.Publish(new CashoutResetHandCountVisibilityChangedEvent(_showDialog));
                RaisePropertyChanged(nameof(ShowDialog));
            }
        }

        public IMessageOverlayData MessageOverlayData { get; set; }

        public CashoutResetHandCountViewModel() : this(
            ServiceManager.GetInstance().TryGetService<IEventBus>(),
            ServiceManager.GetInstance().TryGetService<IContainerService>())
        {
        }

        public CashoutResetHandCountViewModel(IEventBus eventBus,
            IContainerService containerService)
        {
            _eventBus = eventBus;
            MessageOverlayData = containerService.Container.GetInstance<IMessageOverlayData>();
            CashoutResetDialogYesNoCommand = new ActionCommand<object>(CashoutResetDialogYesNoPressed);
            _eventBus.Subscribe<CashOutDialogVisibilityEvent>(this, Handle);
            _eventBus.Subscribe<PayOutLimitVisibility>(this, Handle);
        }

        private void Handle(PayOutLimitVisibility evt)
        {
            MessageOverlayData.IsCashOutDialogVisible = evt.IsVisible;
        }

        private void Handle(CashOutDialogVisibilityEvent evt)
        {
            ShowDialog = true;
        }

        private void CashoutResetDialogYesNoPressed(object obj)
        {
            if (obj.ToString().Equals("YES", StringComparison.InvariantCultureIgnoreCase))
            {
                _eventBus.Publish(new CashOutEvent(true));

            }
            else
            {
                _eventBus.Publish(new CashOutEvent(false));
                MessageOverlayData.IsCashOutDialogVisible = false;
            }
            ShowDialog = false;
        }
    }
}
