namespace Aristocrat.Monaco.Application.UI.Loaders
{
    using Contracts;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using ViewModels;
    using Views;
    using Monaco.Localization.Properties;

    public class BellPageLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BellLabel);

        protected override IOperatorMenuPage CreatePage()
        {
            return new BellPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new BellPageViewModel();
        }

        public override bool GetVisible()
        {
            return (bool)PropertiesManager.GetProperty(ApplicationConstants.ConfigWizardBellConfigurable, false)
                || (bool)PropertiesManager.GetProperty(ApplicationConstants.ConfigWizardBellEnabled, false);
        }
    }
}
