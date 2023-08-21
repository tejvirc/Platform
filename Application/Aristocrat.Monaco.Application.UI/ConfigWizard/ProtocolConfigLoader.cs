namespace Aristocrat.Monaco.Application.UI.ConfigWizard
{
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Loaders;
    using ViewModels;
    using Views;
    using Monaco.Localization.Properties;
    using Aristocrat.Monaco.Application.Contracts.Protocol;
    using Contracts;
    using Kernel;

    /// <summary>
    ///     This class interacts with the configuration screen launcher
    ///     and creates the protocol configuration wizard pages
    /// </summary>
    public class ProtocolConfigLoader : OperatorMenuPageLoader
    {
        /// <inheritdoc/>
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ProtocolScreenTitle);

        protected override IOperatorMenuPage CreatePage()
        {
            return new ProtocolSetupPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new ProtocolSetupPageViewModel(ServiceManager.GetInstance().GetService<IMultiProtocolConfigurationProvider>(), ServiceManager.GetInstance().GetService<IConfigurationUtilitiesProvider>());
        }
    }
}
