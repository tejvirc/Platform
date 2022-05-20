namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using System.Collections.ObjectModel;
    using Application.UI.OperatorMenu;
    using Models;

    public class GameProgressiveWinViewModel : OperatorMenuSaveViewModelBase
    {
        public GameProgressiveWinViewModel(ObservableCollection<ProgressiveWinModel> wins)
        {
            ProgressiveWins = wins;
        }

        public ObservableCollection<ProgressiveWinModel> ProgressiveWins { get; }
    }
}