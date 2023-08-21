namespace Aristocrat.Monaco.G2S.UI.Loaders
{
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.UI.Loaders;
    using Localization.Properties;
    using ViewModels;
    using Views;

    public class SecurityConfigurationViewLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SecurityConfigurationScreenTitle);

        public SecurityConfigurationViewLoader() : this(false) { }

        public SecurityConfigurationViewLoader(bool isWizardPage)
        {
            IsWizardPage = isWizardPage;
        }

        /// <inheritdoc />
        public override CommsProtocol RequiredProtocol => CommsProtocol.G2S;

        protected override IOperatorMenuPage CreatePage()
        {
            return new SecurityConfigurationView { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new SecurityConfigurationViewModel(IsWizardPage);
        }
    }
}
