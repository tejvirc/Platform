namespace Aristocrat.Monaco.Application.UI.Loaders
{
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Monaco.Localization.Properties;
    using ViewModels;
    using Views;

    /// <summary>
    ///     This class interacts with the configuration screen launcher
    ///     and creates the protocol configuration wizard pages
    /// </summary>
    public class MultiProtocolConfigDisplayPageLoader : OperatorMenuPageLoader
    {
        /// <inheritdoc />
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MultiProtocolConfigurationPageTitle);
        protected override IOperatorMenuPage CreatePage()
        {
            return new MultiProtocolConfigDisplayPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new MultiProtocolConfigDisplayPageViewModel();
        }
    }
}