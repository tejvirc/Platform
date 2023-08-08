namespace Aristocrat.Monaco.Accounting.UI.Loaders
{
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.UI.Loaders;
    using Application.Contracts;
    using Kernel;
    using Monaco.Localization.Properties;
    using ViewModels;
    using Views;

    /// <summary>
    ///     HopperPageLoader contains the logic for Loading Hopper Page.
    /// </summary>
    public class HopperPageLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.HopperLabel);

        /// <summary>
        ///     Login to initialise hopper page.
        /// </summary>
        /// <returns></returns>
        protected override IOperatorMenuPage CreatePage()
        {
            return new HopperPage { DataContext = ViewModel };
        }

        /// <summary>
        ///     Login to create hopper Page view model.
        /// </summary>
        /// <returns></returns>
        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new HopperPageViewModel(IsWizardPage);
        }

        public override bool GetVisible()
        {
            return PropertiesManager.GetValue(ApplicationConstants.HopperEnabled, false);
        }
    }
}
