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
    ///     and creates the jurisdiction configuration wizard pages
    /// </summary>
    public class JurisdictionConfigLoader : OperatorMenuPageLoader
    {
        private IOperatorMenuPage _page;

        /// <inheritdoc/>
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.JurisdictionScreenTitle);

        protected override IOperatorMenuPage CreatePage()
        {
            return _page ?? (_page = new JurisdictionSetupPage { DataContext = ViewModel });
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new JurisdictionSetupPageViewModel();
        }
    }
}
