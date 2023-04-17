namespace Aristocrat.Monaco.Application.UI.Loaders
{
    using Contracts;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Kernel;
    using Kernel.Contracts;
    using Monaco.Localization.Properties;
    using ViewModels.NoteAcceptor;
    using Views;

    public class NoteAcceptorPageLoader : OperatorMenuPageLoader
    {
        public override string PageName => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NoteAcceptorLabel);

        public override bool GetVisible()
        {
            return PropertiesManager.GetValue(ApplicationConstants.NoteAcceptorEnabled, false)
                   || !PropertiesManager.GetValue(KernelConstants.IsInspectionOnly, false);
        }

        protected override IOperatorMenuPage CreatePage()
        {
            return new NoteAcceptorPage { DataContext = ViewModel };
        }

        protected override IOperatorMenuPageViewModel CreateViewModel()
        {
            return new NoteAcceptorViewModel(IsWizardPage);
        }
    }
}