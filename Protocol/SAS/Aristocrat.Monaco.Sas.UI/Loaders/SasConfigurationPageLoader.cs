namespace Aristocrat.Monaco.Sas.UI.Loaders
{
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.UI.Loaders;
    using ConfigurationScreen;
    using Localization.Properties;
    using ViewModels;

    /// <inheritdoc />
    public class SasConfigurationPageLoader : OperatorMenuPageLoader
    {
        /// <inheritdoc />
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SasHost);

        /// <inheritdoc />
        public SasConfigurationPageLoader() : this(false) { }

        /// <inheritdoc />
        public SasConfigurationPageLoader(bool isWizardPage)
        {
            IsWizardPage = isWizardPage;
        }

        /// <inheritdoc />
        public override CommsProtocol RequiredProtocol => CommsProtocol.SAS;

        /// <inheritdoc />
        protected override IOperatorMenuPage CreatePage()
        {
            return new SasConfigurationPage { DataContext = ViewModel };
        }

        /// <inheritdoc />
        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new SasConfigurationViewModel(IsWizardPage);
        }
    }
}
