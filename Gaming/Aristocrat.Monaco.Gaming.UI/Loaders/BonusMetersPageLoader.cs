namespace Aristocrat.Monaco.Gaming.UI.Loaders
{
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.UI.Loaders;
    using Localization.Properties;
    using ViewModels.OperatorMenu;
    using Views.OperatorMenu;

    public class BonusMetersPageLoader : OperatorMenuPageLoader
    {
        public override string PageName => GetPageName();

        protected override IOperatorMenuPage CreatePage()
        {
            return new BonusMetersPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new BonusMetersPageViewModel();
        }

        private string GetPageName()
        {
            var pageName = Configuration.GetPageName(this);
            return Localizer.For(CultureFor.Operator)
                .GetString(string.IsNullOrEmpty(pageName) ? ResourceKeys.Bonus : pageName);
        }
    }
}