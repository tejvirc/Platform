namespace Aristocrat.Monaco.Gaming.UI.Loaders
{
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.UI.Loaders;
    using Localization.Properties;
    using ViewModels.OperatorMenu;
    using Views.OperatorMenu;

    internal class GamesMainPageLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Games);

        protected override IOperatorMenuPage CreatePage()
        {
            return new GamesMainPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new GamesMainPageViewModel(PageName);
        }
    }
}
