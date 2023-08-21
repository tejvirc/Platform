namespace Aristocrat.Monaco.Application.UI.Loaders
{
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using ViewModels;
    using Views;
    using Monaco.Localization.Properties;

    public class DiagnosticResourcesPageLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DiagnosticResourcesPageTitle);

        protected override IOperatorMenuPage CreatePage()
        {
            return new DiagnosticResourcesPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new DiagnosticResourcesViewModel();
        }
    }
}
