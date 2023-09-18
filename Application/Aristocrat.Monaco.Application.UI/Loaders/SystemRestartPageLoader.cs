namespace Aristocrat.Monaco.Application.UI.Loaders
{
    using Contracts.OperatorMenu;
    using ViewModels;
    using Views;

    public class SystemRestartPageLoader : OperatorMenuPageLoader
    {
        public override string PageName => "System Restart";

        protected override IOperatorMenuPage CreatePage()
        {
            return new SystemRestartPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new SystemRestartPageViewModel(IsWizardPage);
        }
    }
}
