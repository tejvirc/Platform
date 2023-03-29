namespace Aristocrat.Monaco.Gaming.UI.ViewModels
{
    using MVVM.Command;
    using System;
    using System.Windows.Input;
    using Aristocrat.MVVM.Model;
    using Aristocrat.MVVM.ViewModel;

    public class CashoutResetHandCountWarningViewModel : BaseEntityViewModel
    {

        //private readonly LobbyViewModel _lobby;

        public ICommand CashoutResetDialogYesNoCommand { get; }

        //public CashoutResetHandCountWarningViewModel(LobbyViewModel lobbyViewModel)
        //{
        //    _lobby = lobbyViewModel ?? throw new ArgumentNullException(nameof(lobbyViewModel));
        //    CashoutResetDialogYesNoCommand = new ActionCommand<object>(CashoutResetDialogYesNoPressed);
        //    IsCashOutDialogVisible = true;
        //}
        public CashoutResetHandCountWarningViewModel()
        {
            CashoutResetDialogYesNoCommand = new ActionCommand<object>(CashoutResetDialogYesNoPressed);
        }
        private bool isCashOutDialogVisible;
        public bool IsCashOutDialogVisible
        {
            get => isCashOutDialogVisible;
            set => SetProperty(ref isCashOutDialogVisible, value);
        }

        private void CashoutResetDialogYesNoPressed(object obj)
        {
            if (obj.ToString().Equals("YES", StringComparison.InvariantCultureIgnoreCase))
            {
            }

           // _lobby.IsCashOutDialogVisible = false;
        }
    }
}
