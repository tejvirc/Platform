namespace Aristocrat.Monaco.Application.UI.Loaders
{
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Monaco.Localization.Properties;
    using ViewModels;
    using Views;

    public class OptionsMainPageLoader : OperatorMenuPageLoader
    {
        private readonly string _pageNameResourceKey = ResourceKeys.OptionsScreen;

        public override string PageName => Localizer.For(CultureFor.Operator).GetString(_pageNameResourceKey);

        protected override IOperatorMenuPage CreatePage()
        {
            return new OptionsMainPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new OptionsMainPageViewModel(_pageNameResourceKey);
        }

        public override bool GetVisible()
        {
            return Configuration.GetVisible(this);
        }
    }
}
