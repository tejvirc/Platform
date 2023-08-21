namespace Aristocrat.Monaco.Application.UI.Loaders
{
    using Contracts;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Kernel;
    using ViewModels;
    using Views;
    using Monaco.Localization.Properties;

    public class SystemResetPageLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SystemReset);

        protected override IOperatorMenuPage CreatePage()
        {
            return new SystemResetPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new SystemResetPageViewModel();
        }

        public override bool GetVisible()
        {
            if (PropertiesManager.GetValue(ApplicationConstants.DemonstrationMode, false))
            {
                return false; // No need to show in Demonstration
            }

            return true;
        }
    }
}
