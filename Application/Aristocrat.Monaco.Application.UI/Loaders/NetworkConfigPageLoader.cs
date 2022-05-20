namespace Aristocrat.Monaco.Application.UI.Loaders
{
    using System.Collections.Generic;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using ViewModels;
    using Views;
    using Monaco.Localization.Properties;

    public class NetworkConfigPageLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ConfigNetworkInfoScreen);

        public override List<CommsProtocol> RequiredProtocols => new List<CommsProtocol> { CommsProtocol.G2S, CommsProtocol.MGAM, CommsProtocol.HHR };

        public NetworkConfigPageLoader() : this(false) { }

        public NetworkConfigPageLoader(bool isWizardPage)
        {
            IsWizardPage = isWizardPage;
            IsInstantiated = true;
        }

        protected override IOperatorMenuPage CreatePage()
        {
            return new NetworkConfigPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new NetworkConfigPageViewModel(IsWizardPage);
        }

        public static bool IsInstantiated { get; private set; }
    }
}