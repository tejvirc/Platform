namespace Aristocrat.Monaco.Gaming.UI.ViewModels
{
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Accounting.Contracts.HandCount;
    using Aristocrat.Monaco.Application.Contracts.Extensions;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.MVVM.Command;
    using Aristocrat.MVVM.ViewModel;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Input;

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
                _eventBus.Publish(new CashoutAmountPlayerConfirmationReceivedEvent(true));

            }
            else
            {
                _eventBus.Publish(new CashoutAmountPlayerConfirmationReceivedEvent(false));
            }
        }
    }
}
