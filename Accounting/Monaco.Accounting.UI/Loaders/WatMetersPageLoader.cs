namespace Aristocrat.Monaco.Accounting.UI.Loaders
{
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.UI.Loaders;
    using ViewModels;
    using Views;
    using Monaco.Localization.Properties;

    public class WatMetersPageLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.WatButtonText);

        protected override IOperatorMenuPage CreatePage()
        {
            return new WatMetersPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new WatMetersPageViewModel();
        }
    }
}
