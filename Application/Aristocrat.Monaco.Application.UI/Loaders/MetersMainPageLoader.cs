namespace Aristocrat.Monaco.Application.UI.Loaders
{
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using ViewModels;
    using Views;
    using Monaco.Localization.Properties;

    public class MetersMainPageLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MetersScreen);

        protected override IOperatorMenuPage CreatePage()
        {
            return new MetersMainPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new MetersMainPageViewModel(this);
        }
    }
}
