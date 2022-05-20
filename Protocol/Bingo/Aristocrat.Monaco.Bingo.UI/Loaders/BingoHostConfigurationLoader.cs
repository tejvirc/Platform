namespace Aristocrat.Monaco.Bingo.UI.Loaders
{
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.UI.Loaders;
    using Localization.Properties;
    using ViewModels.OperatorMenu;
    using Views.OperatorMenu;

    public class BingoHostConfigurationLoader : OperatorMenuPageLoader
    {
        public BingoHostConfigurationLoader()
            : this(false)
        {
        }

        public BingoHostConfigurationLoader(bool isWizard)
        {
            IsWizardPage = isWizard;
        }

        public override string PageName =>
            Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BingoHost);

        public override CommsProtocol RequiredProtocol => CommsProtocol.Bingo;

        protected override IOperatorMenuPage CreatePage()
        {
            return new BingoHostConfigurationView { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new BingoHostConfigurationViewModel(IsWizardPage);
        }
    }
}