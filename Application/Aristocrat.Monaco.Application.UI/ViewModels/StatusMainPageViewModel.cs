namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using Aristocrat.Monaco.Application.Contracts.OperatorMenu;
    using OperatorMenu;
    using System;

    [CLSCompliant(false)]
    public class StatusMainPageViewModel : OperatorMenuMultiPageViewModelBase
    {
        private const string MenuExtensionPointPath = "/Application/OperatorMenu/StatusMenu";

        public StatusMainPageViewModel(IOperatorMenuPageLoader mainPage) : base(mainPage, MenuExtensionPointPath)
        {
        }
    }
}
