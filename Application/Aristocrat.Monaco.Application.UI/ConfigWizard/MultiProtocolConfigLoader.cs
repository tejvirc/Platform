namespace Aristocrat.Monaco.Application.UI.ConfigWizard
{
    using Contracts;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Contracts.Protocol;
    using Kernel;
    using Loaders;
    using Monaco.Localization.Properties;
    using ViewModels;
    using Views;

    /// <summary>
    ///     This class interacts with the configuration screen launcher
    ///     and creates the protocol configuration wizard pages
    /// </summary>
    public class MultiProtocolConfigLoader : OperatorMenuPageLoader
    {
        /// <inheritdoc />
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MultiProtocolConfigurationPageTitle);

        protected override IOperatorMenuPage CreatePage()
        {
            return new MultiProtocolConfigPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new MultiProtocolConfigPageViewModel(
                ServiceManager.GetInstance().GetService<IMultiProtocolConfigurationProvider>(),
                ServiceManager.GetInstance().GetService<IProtocolCapabilityAttributeProvider>(),
                ServiceManager.GetInstance().GetService<IConfigurationUtilitiesProvider>());
        }
    }
}