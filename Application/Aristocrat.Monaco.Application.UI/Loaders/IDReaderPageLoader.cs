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

    public class IdReaderPageLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.IdReaderLabel);

        public override bool GetVisible()
        {
            return PropertiesManager.GetValue(ApplicationConstants.IdReaderEnabled, false)
                || !PropertiesManager.GetValue(KernelConstants.IsInspectionOnly, false);
        }

        protected override IOperatorMenuPage CreatePage()
        {
            return new IdReaderPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new IdReaderPageViewModel(IsWizardPage);
        }
    }
}