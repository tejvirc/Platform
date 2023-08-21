namespace Aristocrat.Monaco.Bingo.UI.Loaders
{
    using Application.Contracts.Localization;
    using Application.UI.Loaders;
    using Localization.Properties;
    using Application.Contracts.OperatorMenu;
    using ViewModels.OperatorMenu;
    using Views.OperatorMenu;

    public class BingoServerSettingsLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BingoServerSettingsTitle);

        public override CommsProtocol RequiredProtocol => CommsProtocol.Bingo;

        protected override IOperatorMenuPage CreatePage()
        {
            return new BingoServerSettingsView { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new BingoServerSettingsViewModel();
        }
    }
}
