namespace Aristocrat.Monaco.Application.UI.Loaders
{
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Monaco.Localization.Properties;
    using ViewModels;
    using Views;

    public class GeneralInformationPageLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.GeneralInformationPageTitle);

        protected override IOperatorMenuPage CreatePage()
        {
            return new GeneralInformationPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new GeneralInformationPageViewModel();
        }

        public override bool GetVisible()
        {
            return Configuration.GetSetting(OperatorMenuSetting.ShowGeneralInformationPage, false);
        }
    }
}
