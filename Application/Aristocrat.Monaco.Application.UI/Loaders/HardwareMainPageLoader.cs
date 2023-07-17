namespace Aristocrat.Monaco.Application.UI.Loaders
{
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using ViewModels;
    using Views;
    using Monaco.Localization.Properties;

    public class HardwareMainPageLoader : OperatorMenuPageLoader
    {
        private readonly string _pageNameResourceKey = ResourceKeys.HardwareLabel;

        public override string PageName => Localizer.For(CultureFor.Operator).GetString(_pageNameResourceKey);

        protected override IOperatorMenuPage CreatePage()
        {
            return new HardwareMainPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new HardwareMainPageViewModel(_pageNameResourceKey);
        }
    }
}
