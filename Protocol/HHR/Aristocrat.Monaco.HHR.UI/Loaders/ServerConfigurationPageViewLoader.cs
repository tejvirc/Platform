namespace Aristocrat.Monaco.Hhr.UI.Loaders
{
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.UI.Loaders;
    using Localization.Properties;
    using ViewModels;
    using Views;

    // ReSharper disable once UnusedMember.Global
    public class ServerConfigurationPageViewLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ServerConfigurationPageTitle);

        public override CommsProtocol RequiredProtocol => CommsProtocol.HHR;

        public ServerConfigurationPageViewLoader()
            : this(false)
        {
        }

        public ServerConfigurationPageViewLoader(bool isWizardPage)
        {
            IsWizardPage = isWizardPage;
        }

        protected override IOperatorMenuPage CreatePage()
        {
            return new ServerConfigurationPageView { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new ServerConfigurationPageViewModel(IsWizardPage);
        }
    }
}