namespace Aristocrat.Monaco.Application.UI.Loaders
{
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using ViewModels;
    using Views;
    using Monaco.Localization.Properties;

    public class DisplaysPageLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.TouchScreenPixelLabel);

        public override bool GetVisible()
        {
            return true;
        }

        protected override IOperatorMenuPage CreatePage()
        {
            return new DisplaysPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new DisplaysPageViewModel(IsWizardPage);
        }
    }
}