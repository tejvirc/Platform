namespace Aristocrat.Monaco.Application.UI.Loaders
{
    using Contracts;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Kernel;
    using Monaco.Localization.Properties;
    using ViewModels;
    using Views;

    /// <summary>
    ///     CoinAcceptorPageLoader contains the logic for Loading coin acceptor page.
    /// </summary>
    public class CoinAcceptorPageLoader
        : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CoinAcceptorLabel);

        /// <summary>
        ///     Login to initialise coin acceptor page.
        /// </summary>
        /// <returns></returns>
        protected override IOperatorMenuPage CreatePage()
        {
            return new CoinAcceptorPage { DataContext = ViewModel };
        }

        /// <summary>
        ///     Login to create CoinAcceptor Page view model.
        /// </summary>
        /// <returns></returns>
        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new CoinAcceptorPageViewModel();
        }
        /// <inheritdoc />
        public override bool GetVisible()
        {
            return PropertiesManager.GetValue(ApplicationConstants.CoinAcceptorEnabled, false);
        }
    }
}
