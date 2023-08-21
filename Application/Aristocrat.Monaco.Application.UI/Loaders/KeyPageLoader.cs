namespace Aristocrat.Monaco.Application.UI.Loaders
{
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using ViewModels;
    using Views;
    using Monaco.Localization.Properties;

    public class KeyPageLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.KeysLabel);

        public override bool GetVisible()
        {
            return true;
        }

        protected override IOperatorMenuPage CreatePage()
        {
            return new KeyPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new KeyPageViewModel(IsWizardPage);
        }
    }
}