namespace Aristocrat.Monaco.Accounting.UI.Loaders
{
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.UI.Loaders;
    using ViewModels;
    using Views;
    using Monaco.Localization.Properties;

    public class CurrentCreditsPageLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CurrentCredits);

        protected override IOperatorMenuPage CreatePage()
        {
            return new CurrentCreditsPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new CurrentCreditsPageViewModel();
        }
    }
}
