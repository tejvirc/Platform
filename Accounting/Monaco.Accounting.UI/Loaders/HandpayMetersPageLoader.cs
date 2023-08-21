namespace Aristocrat.Monaco.Accounting.UI.Loaders
{
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.UI.Loaders;
    using Monaco.Localization.Properties;
    using ViewModels;
    using Views;

    public class HandpayMetersPageLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.Player).GetString(ResourceKeys.Handpay);

        protected override IOperatorMenuPage CreatePage()
        {
            return new HandpayMetersPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new HandpayMetersPageViewModel();
        }
    }
}