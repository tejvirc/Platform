namespace Aristocrat.Monaco.Application.UI.Loaders
{
    using Contracts;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Kernel;
    using Kernel.Contracts;
    using ViewModels;
    using Views;
    using Monaco.Localization.Properties;

    public class PrinterPageLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PrinterLabel);

        public override bool GetVisible()
        {
            return PropertiesManager.GetValue(ApplicationConstants.PrinterEnabled, false)
                   || !PropertiesManager.GetValue(KernelConstants.IsInspectionOnly, false);
        }

        protected override IOperatorMenuPage CreatePage()
        {
            return new PrinterPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new PrinterViewModel(IsWizardPage);
        }
    }
}