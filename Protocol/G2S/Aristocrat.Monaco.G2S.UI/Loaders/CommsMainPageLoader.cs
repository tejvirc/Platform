namespace Aristocrat.Monaco.G2S.UI.Loaders
{
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.UI.Loaders;
    using Localization.Properties;
    using ViewModels;
    using Views;

    public class CommsMainPageLoader : OperatorMenuPageLoader
    {
        private readonly string _pageNameResourceKey = ResourceKeys.HostsInfoViewTitle;

        public override string PageName => Localizer.For(CultureFor.Operator).GetString(_pageNameResourceKey);

        protected override IOperatorMenuPage CreatePage()
        {
            return new CommsMainPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new CommsMainPageViewModel(_pageNameResourceKey);
        }
    }
}
