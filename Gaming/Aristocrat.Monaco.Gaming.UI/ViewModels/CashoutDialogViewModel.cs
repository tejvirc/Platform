namespace Aristocrat.Monaco.Gaming.UI.ViewModels
{
    using System;
    using System.Windows.Input;
    using Accounting.Contracts.HandCount;
    using Contracts;
    using Kernel;
    using MVVM.Command;
    using MVVM.ViewModel;
    using Runtime;
    using Runtime.Client;

    public class CashoutDialogViewModel : BaseEntityViewModel
    {
        private readonly IEventBus _eventBus;
        private long _handCountAmount;

        public ICommand CashoutDialogYesNoCommand { get; }

        public long HandCountAmount
        {
            get
            {
                return _handCountAmount;
            }
            set
            {
                _handCountAmount = value;
                RaisePropertyChanged(nameof(HandCountAmount));
            }
        }

        public CashoutDialogViewModel() : this(
            ServiceManager.GetInstance().TryGetService<IEventBus>())
        {
        }

        public CashoutDialogViewModel(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            CashoutDialogYesNoCommand = new ActionCommand<object>(CashoutDialogYesNoPressed);
        }

        private void CashoutDialogYesNoPressed(object obj)
        {
            if (obj.ToString().Equals("YES", StringComparison.InvariantCultureIgnoreCase))
            {
                _eventBus.Publish(new CashoutAmountAuthorizationReceivedEvent(true));
            }
            else
            {
                _eventBus.Publish(new CashoutAmountAuthorizationReceivedEvent(false));
                var runtimeFlagHandler = ServiceManager.GetInstance().GetService<IContainerService>()?.Container.GetInstance<IRuntimeFlagHandler>();
                runtimeFlagHandler?.SetCashingOut(false);
            }
        }
    }
}
