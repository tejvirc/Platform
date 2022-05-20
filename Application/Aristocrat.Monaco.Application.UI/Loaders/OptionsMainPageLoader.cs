namespace Aristocrat.Monaco.Application.UI.Loaders
{
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using ViewModels;
    using Views;
    using Monaco.Localization.Properties;

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
            // If the Accounting Config page is visible, we need the options multi-page
            return Configuration.GetVisible(OperatorMenuSetting.AccountingConfigurationLoader);
        }
    }
}
