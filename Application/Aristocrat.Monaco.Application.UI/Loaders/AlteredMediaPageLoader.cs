namespace Aristocrat.Monaco.Application.UI.Loaders
{
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using ViewModels;
    using Views;
    using Monaco.Localization.Properties;

    public class AlteredMediaPageLoader : OperatorMenuPageLoader
    {
        public override string PageName => GetPageName();

        protected override IOperatorMenuPage CreatePage()
        {
            return new AlteredMediaLog { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new AlteredMediaLogViewModel();
        }

        private string GetPageName()
        {
            var pageName = Configuration.GetPageName(this);
            return Localizer.For(CultureFor.Operator).GetString(string.IsNullOrEmpty(pageName) ? ResourceKeys.AlteredMediaPageLoaderText : pageName);
        }
    }
}
