namespace Aristocrat.Monaco.Gaming.UI.Loaders
{
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.UI.Loaders;
    using Aristocrat.Monaco.Application.Contracts;
    using Kernel;
    using Localization.Properties;
    using ViewModels.OperatorMenu;
    using Views.OperatorMenu;

    public class CentralDeterminationLogLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CentralDeterminationLogTitle);

        protected override IOperatorMenuPage CreatePage()
        {
            return new CentralDeterminationLogView { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new CentralDeterminationLogViewModel();
        }

        public override bool GetVisible()
        {
            return PropertiesManager.GetValue(ApplicationConstants.CentralAllowed, false);
        }
    }
}