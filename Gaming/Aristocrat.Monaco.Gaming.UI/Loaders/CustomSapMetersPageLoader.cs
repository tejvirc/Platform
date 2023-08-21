namespace Aristocrat.Monaco.Gaming.UI.Loaders
{
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.UI.Loaders;
    using Localization.Properties;
    using ViewModels.OperatorMenu;
    using Views.OperatorMenu;

    public class CustomSAPMetersPageLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CustomSAPTitle);

        protected override IOperatorMenuPage CreatePage()
        {
            return new CustomSAPMetersPage
            {
                DataContext = ViewModel
            };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new CustomSAPMetersPageViewModel();
        }
    }
}
