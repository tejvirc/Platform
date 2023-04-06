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
        
        public CashoutResetHandCountWarningViewModel() : this(ServiceManager.GetInstance().TryGetService<IEventBus>())
        {
        }

        public CashoutResetHandCountWarningViewModel(IEventBus eventBus)
        {
            _eventBus = eventBus;
            CashoutResetDialogYesNoCommand = new ActionCommand<object>(CashoutResetDialogYesNoPressed);
            _eventBus.Subscribe<HandCountCashoutEvent>(this, Handle);
        }

        private void Handle(HandCountCashoutEvent obj)
        {
            ShowDialog = true;
        }

        private void CashoutResetDialogYesNoPressed(object obj)
        {
            if (obj.ToString().Equals("YES", StringComparison.InvariantCultureIgnoreCase))
            {
                _eventBus.Publish(new HandCountDialogEvent(true));
            }
            else
            {
                _eventBus.Publish(new HandCountDialogEvent(false));
            }
            ShowDialog = false;
        }
    }
}
