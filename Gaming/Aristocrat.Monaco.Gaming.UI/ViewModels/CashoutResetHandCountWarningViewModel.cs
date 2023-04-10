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

    public class CashoutResetHandCountWarningViewModel : BaseEntityViewModel
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
        
        public CashoutResetHandCountWarningViewModel() : this(
            ServiceManager.GetInstance().TryGetService<IEventBus>(),
            ServiceManager.GetInstance().TryGetService<IContainerService>())
        {
        }

        public CashoutResetHandCountWarningViewModel(IEventBus eventBus,
            IContainerService containerService)
        {
            _eventBus = eventBus;
            MessageOverlayData = containerService.Container.GetInstance<IMessageOverlayData>();
            CashoutResetDialogYesNoCommand = new ActionCommand<object>(CashoutResetDialogYesNoPressed);
            _eventBus.Subscribe<HandCountCashoutEvent>(this, Handle);
            _eventBus.Subscribe<CashOutVisiblEventcs>(this, Handle);
        }

        private void Handle(CashOutVisiblEventcs obj)
        {
            MessageOverlayData.IsCashOutDialogVisible = obj.IsVisible;
        }

        private void Handle(HandCountCashoutEvent obj)
        {
            ShowDialog = true;
        }

        public IMessageOverlayData MessageOverlayData { get; set; }

        private void CashoutResetDialogYesNoPressed(object obj)
        {
            if (obj.ToString().Equals("YES", StringComparison.InvariantCultureIgnoreCase))
            {
                _eventBus.Publish(new HandCountDialogEvent(true));
               
            }
            else
            {
                _eventBus.Publish(new HandCountDialogEvent(false));
                MessageOverlayData.IsCashOutDialogVisible = false;
            }
            ShowDialog = false;
        }
    }
}
