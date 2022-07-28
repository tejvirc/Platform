namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using OperatorMenu;

    [CLSCompliant(false)]
    public class DiagnosticMainPageViewModel : OperatorMenuMultiPageViewModelBase
    {

        private const string PagesExtensionPath = @"/Application/OperatorMenu/DiagnosticMenu";

        public DiagnosticMainPageViewModel(string displayPageTitle) : base(displayPageTitle, PagesExtensionPath)
        {

        }



    }
}
