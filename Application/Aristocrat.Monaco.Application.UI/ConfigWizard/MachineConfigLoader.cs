namespace Aristocrat.Monaco.Application.UI.ConfigWizard
{
    using Contracts;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Kernel;
    using Loaders;
    using ViewModels;
    using Views;
    using Monaco.Localization.Properties;

    /// <summary>
    ///     This class interacts with the configuration screen launcher
    ///     and creates the jurisdiction configuration wizard pages
    /// </summary>
    public class MachineConfigLoader : OperatorMenuPageLoader
    {
        /// <inheritdoc/>
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MachineSetup);

        protected override IOperatorMenuPage CreatePage()
        {
            return new MachineSetupPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new MachineSetupPageViewModel();
        }

        public override bool GetVisible()
        {
            return PropertiesManager.GetValue(ApplicationConstants.ConfigWizardMachineSetupConfigVisibility, false);
        }
    }
}
