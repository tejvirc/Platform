namespace Aristocrat.Monaco.Gaming.UI.Loaders
{
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.UI.Loaders;
    using Localization.Properties;
    using ViewModels.OperatorMenu;
    using Views.OperatorMenu;

    public class EventLogFilterPageLoader : OperatorMenuPageLoader
    {
        public override string PageName => GetPageName();

        protected override IOperatorMenuPage CreatePage()
        {
            return new EventLogFilterPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new EventLogFilterViewModel();
        }

        private string GetPageName()
        {
            var pageName = Configuration.GetPageName(this);
            return Localizer.For(CultureFor.Operator).GetString(string.IsNullOrEmpty(pageName) ? ResourceKeys.EventLogTitle : pageName);
        }
    }
}
