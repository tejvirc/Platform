namespace Aristocrat.Monaco.Application.UI.ConfigWizard
{
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Loaders;
    using ViewModels;
    using Views;
    using Monaco.Localization.Properties;

    public class CompletionPageLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CompleteTitle);

        protected override IOperatorMenuPage CreatePage()
        {
            return new CompletionPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new CompletionPageViewModel();
        }
    }
}
