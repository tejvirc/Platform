namespace Aristocrat.Monaco.G2S.UI.Loaders
{
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.UI.Loaders;
    using Localization.Properties;
    using ViewModels;
    using Views;

    public class DeviceManagerViewLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DeviceManagerTitle);

        public override CommsProtocol RequiredProtocol => CommsProtocol.G2S;

        protected override IOperatorMenuPage CreatePage()
        {
            return new DeviceManagerView { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new DeviceManagerViewModel();
        }
    }
}
