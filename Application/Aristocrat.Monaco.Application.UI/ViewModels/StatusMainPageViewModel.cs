namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using Contracts.OperatorMenu;
    using OperatorMenu;

    [CLSCompliant(false)]
    public class StatusMainPageViewModel : OperatorMenuMultiPageViewModelBase
    {
        private const string MenuExtensionPointPath = "/Application/OperatorMenu/StatusMenu";

        public StatusMainPageViewModel(IOperatorMenuPageLoader mainPage) : base(mainPage, MenuExtensionPointPath)
        {
        }
    }
}
