namespace Aristocrat.Monaco.Application.UI.Loaders
{
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Kernel.Contracts;
    using Monaco.Localization.Properties;
    using ViewModels;
    using Views;

    public class TimeConfigPageLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(
            (bool)PropertiesManager.GetProperty(KernelConstants.IsInspectionOnly, false)
            ? ResourceKeys.InspectionSetup : ResourceKeys.DateAndTime);

        protected override IOperatorMenuPage CreatePage()
        {
            return new TimeConfigPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new TimeConfigPageViewModel(IsWizardPage);
        }
    }
}
