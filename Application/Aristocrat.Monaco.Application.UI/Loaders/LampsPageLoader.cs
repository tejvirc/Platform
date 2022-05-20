namespace Aristocrat.Monaco.Application.UI.Loaders
{
    using Contracts;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Hardware.Contracts.ButtonDeck;
    using Kernel;
    using ViewModels;
    using Views;
    using Monaco.Localization.Properties;

    public class LampsPageLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LampsLabel);

        protected override IOperatorMenuPage CreatePage()
        {
            return new LampsPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new LampsPageViewModel();
        }

        /// <inheritdoc />
        public override bool GetVisible()
        {
            var towerLightManager = ServiceManager.GetInstance().GetService<ITowerLightManager>();
            return (ButtonDeckUtilities.HasLamps() || !towerLightManager.TowerLightsDisabled) && base.GetVisible();
        }
    }
}