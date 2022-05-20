namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using OperatorMenu;
    using System;

    [CLSCompliant(false)]
    public class StatusMainPageViewModel : OperatorMenuMultiPageViewModelBase
    {
        private const string MenuExtensionPointPath = "/Application/OperatorMenu/StatusMenu";

        public StatusMainPageViewModel(string displayPageTitle) : base(displayPageTitle, MenuExtensionPointPath)
        {
        }
    }
}
