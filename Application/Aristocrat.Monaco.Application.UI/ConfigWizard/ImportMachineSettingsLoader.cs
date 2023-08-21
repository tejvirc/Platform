namespace Aristocrat.Monaco.Application.UI.ConfigWizard
{
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Loaders;
    using Monaco.Localization.Properties;
    using ViewModels;
    using Views;

    public class ImportMachineSettingsLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ImportSettingsLabel);

        protected override IOperatorMenuPage CreatePage()
        {
            return new ImportMachineSettingsPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new ImportMachineSettingsViewModel();
        }
    }
}
