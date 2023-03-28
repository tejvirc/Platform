namespace Aristocrat.Monaco.Application.UI.ConfigWizard
{
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Loaders;
    using ViewModels;
    using Views;
    using Monaco.Localization.Properties;

    public class InspectionSummaryPageLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InspectionSummaryTitle);

        protected override IOperatorMenuPage CreatePage()
        {
            return new InspectionSummaryPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new InspectionSummaryPageViewModel();
        }
    }
}
