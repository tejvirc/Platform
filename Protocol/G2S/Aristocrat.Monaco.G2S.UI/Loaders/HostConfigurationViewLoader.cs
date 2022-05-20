namespace Aristocrat.Monaco.G2S.UI.Loaders
{
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.UI.Loaders;
    using Localization.Properties;
    using ViewModels;
    using Views;

    public class HostConfigurationViewLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.HostConfigurationPageTitle);

        public override CommsProtocol RequiredProtocol => CommsProtocol.G2S;

        public HostConfigurationViewLoader() : this(false) { }

        public HostConfigurationViewLoader(bool isWizardPage)
        {
            IsWizardPage = isWizardPage;
        }

        protected override IOperatorMenuPage CreatePage()
        {
            return new HostConfigurationView { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new HostConfigurationViewModel(IsWizardPage);
        }
    }
}
