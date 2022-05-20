namespace Aristocrat.Monaco.Application.UI.ConfigWizard
{
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Loaders;
    using ViewModels;
    using Views;
    using Monaco.Localization.Properties;

    /// <summary>
    ///     This class interacts with the configuration screen launcher
    ///     and provides information about this configuration wizard
    /// </summary>
    public class HardwareConfigLoader : OperatorMenuPageLoader
    {
        /// <inheritdoc/>
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.HardwareSelectionScreen);

        protected override IOperatorMenuPage CreatePage()
        {
            return new HardwareConfigPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new HardwareConfigPageViewModel();
        }
    }
}
