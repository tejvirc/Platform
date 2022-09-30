namespace Aristocrat.Monaco.Application.UI.Loaders
{
    using Contracts;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Monaco.Localization.Properties;
    using ViewModels;
    using Views;

    public class BeagleBonePageLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LsEdgeLightsLabel);

        protected override IOperatorMenuPage CreatePage()
        {
            return new BeagleBonePage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new BeagleBonePageViewModel();
        }

        public override bool GetVisible()
        {
            return (bool)PropertiesManager.GetProperty(ApplicationConstants.BeagleBoneEnabled, false);
        }
    }
}
