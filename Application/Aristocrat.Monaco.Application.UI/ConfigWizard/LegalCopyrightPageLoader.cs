namespace Aristocrat.Monaco.Application.UI.ConfigWizard
{
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Loaders;
    using Monaco.Localization.Properties;
    using ViewModels;
    using Views;

    public class LegalCopyrightPageLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LegalCopyrightScreenTitle);

        protected override IOperatorMenuPage CreatePage()
        {
            return new LegalCopyrightPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new LegalCopyrightPageViewModel();
        }
    }
}
