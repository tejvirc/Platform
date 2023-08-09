namespace Aristocrat.Monaco.Gaming.UI.ViewModels
{
    using System;
    using System.Windows.Input;
    using Accounting.Contracts.HandCount;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using Contracts;
    using Kernel;
    using Runtime;
    using Runtime.Client;

    public class CashoutDialogViewModel : ObservableObject
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
                OnPropertyChanged(nameof(HandCountAmount));
            }
        }

        public CashoutDialogViewModel() : this(
            ServiceManager.GetInstance().TryGetService<IEventBus>())
        {
        }

        public CashoutDialogViewModel(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            CashoutDialogYesNoCommand = new RelayCommand<object>(CashoutDialogYesNoPressed);
        }

        private void CashoutDialogYesNoPressed(object obj)
        {
            var runtime = ServiceManager.GetInstance().GetService<IContainerService>().Container.GetInstance<IRuntime>();
            if (obj.ToString().Equals("YES", StringComparison.InvariantCultureIgnoreCase))
            {
                _eventBus.Publish(new CashoutAmountAuthorizationReceivedEvent(true));

            }
            else
            {
                _eventBus.Publish(new CashoutAmountAuthorizationReceivedEvent(false));
                runtime.UpdateFlag(RuntimeCondition.CashingOut, false);
            }
        }
    }
}
