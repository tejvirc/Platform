namespace Aristocrat.Monaco.Accounting.UI.Loaders
{
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.UI.Loaders;
    using ViewModels;
    using Views;
    using Monaco.Localization.Properties;

    public class BillsMetersPageLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.Player).GetString(ResourceKeys.MetersBills);

        protected override IOperatorMenuPage CreatePage()
        {
            return new BillsMetersPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new BillsMetersPageViewModel(PageName);
        }
    }
}
