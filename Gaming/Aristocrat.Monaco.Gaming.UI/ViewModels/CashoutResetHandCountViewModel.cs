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

        public ICommand CashoutResetDialogYesNoCommand { get; }

        public CashoutResetHandCountViewModel() : this(
            ServiceManager.GetInstance().TryGetService<IEventBus>())
        {
        }

        public CashoutResetHandCountViewModel(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            CashoutResetDialogYesNoCommand = new ActionCommand<object>(CashoutResetDialogYesNoPressed);
        }

        private void CashoutResetDialogYesNoPressed(object obj)
        {
            if (obj.ToString().Equals("YES", StringComparison.InvariantCultureIgnoreCase))
            {
                _eventBus.Publish(new CashoutAmountPlayerConfirmationReceivedEvent(true));

            }
            else
            {
                _eventBus.Publish(new CashoutAmountPlayerConfirmationReceivedEvent(false));
                _eventBus.Publish(new PayOutLimitVisibility(false));
            }
        }
    }
}
