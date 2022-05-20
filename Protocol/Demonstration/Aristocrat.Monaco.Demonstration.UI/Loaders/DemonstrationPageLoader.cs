namespace Aristocrat.Monaco.Demonstration.UI.Loaders
{
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.UI.Loaders;
    using Kernel;
    using Localization.Properties;
    using ViewModels;
    using Views;

    public class DemonstrationPageLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DemonstrationPage);

        protected override IOperatorMenuPage CreatePage()
        {
            return new DemonstrationPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new DemonstrationPageViewModel();
        }

        public override bool GetVisible()
        {
            return PropertiesManager.GetValue(ApplicationConstants.DemonstrationMode, false);
        }
    }
}
