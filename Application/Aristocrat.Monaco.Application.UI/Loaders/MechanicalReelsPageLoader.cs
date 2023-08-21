namespace Aristocrat.Monaco.Application.UI.Loaders
{
    using Contracts;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Kernel;
    using ViewModels;
    using Views;
    using Monaco.Localization.Properties;

    public class MechanicalReelsPageLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MechanicalReelsLabel);

        public override bool GetVisible()
        {
            return PropertiesManager.GetValue(ApplicationConstants.ReelControllerEnabled, false);
        }

        protected override IOperatorMenuPage CreatePage()
        {
            return new MechanicalReelsPage() { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new MechanicalReelsPageViewModel(IsWizardPage);
        }
    }
}
