namespace Aristocrat.Monaco.Accounting.UI.Loaders
{
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.UI.Loaders;
    using ViewModels;
    using Views;
    using Monaco.Localization.Properties;

    public class KeyedCreditsPageLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.KeyedCreditsTitle);

        public override CommsProtocol RequiredProtocol => CommsProtocol.SAS;

        protected override IOperatorMenuPage CreatePage()
        {
            return new KeyedCreditsPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new KeyedCreditsPageViewModel();
        }
    }
}