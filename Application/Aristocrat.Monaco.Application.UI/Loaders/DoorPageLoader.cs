namespace Aristocrat.Monaco.Application.UI.Loaders
{
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using ViewModels;
    using Views;
    using Monaco.Localization.Properties;

    public class DoorPageLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DoorLabel);

        public override bool GetVisible()
        {
            return true;
        }

        protected override IOperatorMenuPage CreatePage()
        {
            return new DoorPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new DoorPageViewModel(IsWizardPage);
        }
    }
}