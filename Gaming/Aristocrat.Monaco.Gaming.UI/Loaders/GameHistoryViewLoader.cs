namespace Aristocrat.Monaco.Gaming.UI.Loaders
{
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.UI.Loaders;
    using Localization.Properties;
    using ViewModels.OperatorMenu;
    using Views.OperatorMenu;

    public class GameHistoryViewLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.GameHistoryTitle);

        protected override IOperatorMenuPage CreatePage()
        {
            return new GameHistoryView { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new GameHistoryViewModel();
        }

        public override bool GetVisible()
        {
            // If not a subpage, check GamesPage visibility to determine if games main page is visible
            // If it is, do not also include Game History page as a non-subpage
            var historyPageVisible = Configuration.GetVisible(this);
            return IsSubPage
                ? historyPageVisible
                : !Configuration.GetVisible(typeof(GamesMainPageLoader)) && historyPageVisible;
        }
    }
}
