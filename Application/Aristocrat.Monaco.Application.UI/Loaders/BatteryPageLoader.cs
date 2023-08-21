namespace Aristocrat.Monaco.Application.UI.Loaders
{
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using ViewModels;
    using Views;
    using Monaco.Localization.Properties;

    public class BatteryPageLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BatteriesLabel);

        public override bool GetVisible()
        {
            return true;
        }

        protected override IOperatorMenuPage CreatePage()
        {
            return new BatteryPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new BatteryPageViewModel(IsWizardPage);
        }
    }
}