namespace Aristocrat.Monaco.Sas.UI.Loaders
{
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.UI.Loaders;
    using ConfigurationScreen;
    using Localization.Properties;
    using ViewModels;

    /// <inheritdoc />
    public class SasFeaturesPageLoader : OperatorMenuPageLoader
    {
        /// <inheritdoc />
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SasConfigurationTitle);

        /// <inheritdoc />
        public override CommsProtocol RequiredProtocol => CommsProtocol.SAS;

        /// <inheritdoc />
        public SasFeaturesPageLoader() : this(false) { }

        /// <inheritdoc />
        public SasFeaturesPageLoader(bool isWizardPage)
        {
            IsWizardPage = isWizardPage;
        }

        /// <inheritdoc />
        protected override IOperatorMenuPage CreatePage()
        {
            return new SasFeaturesPage { DataContext = ViewModel };
        }

        /// <inheritdoc />
        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new SasFeatureViewModel(IsWizardPage);
        }
    }
}
