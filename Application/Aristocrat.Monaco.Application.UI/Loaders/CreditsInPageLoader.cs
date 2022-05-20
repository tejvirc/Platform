namespace Aristocrat.Monaco.Application.UI.Loaders
{
    using Contracts.OperatorMenu;
    using ViewModels;
    using Views;

    public class CreditsInPageLoader : OperatorMenuPageLoader
    {
        public override string PageName => null;

        protected override IOperatorMenuPage CreatePage()
        {
            return new CreditsInPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new CreditsInPageViewModel();
        }

        public override bool GetVisible()
        {
            return Configuration.GetSetting(typeof(StatusPageViewModel), OperatorMenuSetting.OperatorCanOverrideMaxCreditsIn, false);
        }
    }
}