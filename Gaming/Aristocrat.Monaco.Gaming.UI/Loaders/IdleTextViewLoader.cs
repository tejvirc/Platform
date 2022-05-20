namespace Aristocrat.Monaco.Gaming.UI.Loaders
{
    using Application.Contracts.OperatorMenu;
    using Application.UI.Loaders;
    using ViewModels.OperatorMenu;
    using Views.OperatorMenu;

    public class IdleTextViewLoader : OperatorMenuPageLoader
    {
        public override string PageName => null;

        protected override IOperatorMenuPage CreatePage()
        {
            return new IdleTextView { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new IdleTextViewModel();
        }
    }
}