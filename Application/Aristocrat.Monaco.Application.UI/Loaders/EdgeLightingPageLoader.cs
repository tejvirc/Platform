namespace Aristocrat.Monaco.Application.UI.Loaders
{
    using Contracts;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Monaco.Localization.Properties;
    using ViewModels;
    using Views;

    public class EdgeLightingPageLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LightingLabel);

        public override bool GetVisible()
        {
            return true;
        }

        protected override IOperatorMenuPage CreatePage()
        {
            return new EdgeLightingPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new EdgeLightingPageViewModel(IsWizardPage);
        }

        public override bool GetVisible()
        {
            // Disable this page for the LS cabinet
            return (bool)PropertiesManager.GetProperty(ApplicationConstants.DisplayLightingPage, false);
        }
    }
}