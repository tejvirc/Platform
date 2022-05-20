namespace Aristocrat.Monaco.Application.UI.Loaders
{
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Monaco.Localization.Properties;
    using ViewModels;
    using Views;

    public class LicensePageLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.License);

        protected override IOperatorMenuPage CreatePage()
        {
            return new Licensing { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new LicenseViewModel();
        }
    }
}