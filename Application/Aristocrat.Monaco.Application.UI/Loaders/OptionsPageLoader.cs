namespace Aristocrat.Monaco.Application.UI.Loaders
{
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using ViewModels;
    using Views;
    using Monaco.Localization.Properties;

    public class OptionsPageLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OptionsScreen);

        protected override IOperatorMenuPage CreatePage()
        {
            return new OptionsPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new OptionsPageViewModel(PageName);
        }

        public override bool GetVisible()
        {
            // If not a subpage, check AccountingConfig visibility to determine if options main page is visible
            // If it is, do not also include Options page as a non-subpage
            var optionsPageVisible = Configuration.GetVisible(this);
            return IsSubPage
                ? optionsPageVisible
                : !Configuration.GetVisible(OperatorMenuSetting.AccountingConfigurationLoader) && optionsPageVisible;
        }
    }
}
