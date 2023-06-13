namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using Application.UI.OperatorMenu;
    using Aristocrat.Monaco.Application.Contracts.OperatorMenu;

    public class GamesMainPageViewModel : OperatorMenuMultiPageViewModelBase
    {
        private const string PagesExtensionPath = "/Application/OperatorMenu/GamesMenu";

        public GamesMainPageViewModel(IOperatorMenuPageLoader mainPage) : base(mainPage, PagesExtensionPath)
        {
        }
    }
}
