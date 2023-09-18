namespace Aristocrat.Monaco.Application.UI.Loaders
{
    using System;
    using Aristocrat.Monaco.Application.Contracts;
    using Aristocrat.Monaco.Kernel;
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
