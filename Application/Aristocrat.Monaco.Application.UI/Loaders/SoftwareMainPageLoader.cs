namespace Aristocrat.Monaco.Application.UI.Loaders
{
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using ViewModels;
    using Views;
    using Monaco.Localization.Properties;

    public class SoftwareMainPageLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Software);

        protected override IOperatorMenuPage CreatePage()
        {
            return new SoftwareMainPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new SoftwareMainPageViewModel(this);
        }
    }
}
