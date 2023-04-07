namespace Aristocrat.Monaco.Application.UI.Loaders
{
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Monaco.Localization.Properties;
    using ViewModels;
    using Views;

    public class OptionsMainPageLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OptionsScreen);

        protected override IOperatorMenuPage CreatePage()
        {
            return new OptionsMainPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new OptionsMainPageViewModel(PageName);
        }

        public override bool GetVisible()
        {
            return Configuration.GetVisible(this);
        }
    }
}
