namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using Application.UI.OperatorMenu;

    public class GamesMainPageViewModel : OperatorMenuMultiPageViewModelBase
    {
        private const string PagesExtensionPath = "/Application/OperatorMenu/GamesMenu";

        public GamesMainPageViewModel(string displayPageTitle) : base(displayPageTitle, PagesExtensionPath)
        {
        }
    }
}
