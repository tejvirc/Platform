namespace Aristocrat.Monaco.Application.UI.Loaders
{
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Monaco.Localization.Properties;
    using ViewModels.NoteAcceptor;
    using Views;

    public class NoteAcceptorPageLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NoteAcceptorLabel);

        protected override IOperatorMenuPage CreatePage()
        {
            return new NoteAcceptorPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new NoteAcceptorViewModel();
        }
    }
}