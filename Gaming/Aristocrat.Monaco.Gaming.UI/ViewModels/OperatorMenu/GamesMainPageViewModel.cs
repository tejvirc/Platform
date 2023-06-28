namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using Application.Contracts.OperatorMenu;
    using Application.UI.OperatorMenu;

    public class GamesMainPageViewModel : OperatorMenuMultiPageViewModelBase
    {
        private const string PagesExtensionPath = "/Application/OperatorMenu/GamesMenu";

        public GamesMainPageViewModel(IOperatorMenuPageLoader mainPage) : base(mainPage, PagesExtensionPath)
        {
        }
    }
}
