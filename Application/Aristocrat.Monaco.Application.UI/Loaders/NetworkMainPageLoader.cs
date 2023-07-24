namespace Aristocrat.Monaco.Application.UI.Loaders
{
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using ViewModels;
    using Views;
    using Monaco.Localization.Properties;

    public class NetworkMainPageLoader : OperatorMenuPageLoader
    {
        private readonly string _pageNameResourceKey = ResourceKeys.NetworkInfoConfiguration;

        public override string PageName => Localizer.For(CultureFor.Operator).GetString(_pageNameResourceKey);

        protected override IOperatorMenuPage CreatePage()
        {
            return new NetworkMainPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new NetworkMainPageViewModel(_pageNameResourceKey);
        }
    }
}
