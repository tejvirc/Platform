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
    using Localization;
    using Hardware.Contracts.NoteAcceptor;

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
            var serviceManager = ServiceManager.GetInstance();
            var localization = serviceManager.GetService<ILocalization>();
            var currencyProvider = localization.GetProvider(CultureFor.Currency) as CurrencyCultureProvider;
            
            return new MachineSetupPageViewModel(currencyProvider);
        }

        public override bool GetVisible()
        {
            return PropertiesManager.GetValue(ApplicationConstants.ConfigWizardMachineSetupConfigVisibility, false);
        }
    }
}
