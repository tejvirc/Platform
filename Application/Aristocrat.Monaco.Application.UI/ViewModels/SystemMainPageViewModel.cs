namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using Contracts.OperatorMenu;
    using OperatorMenu;

    [CLSCompliant(false)]
    public class SystemMainPageViewModel : OperatorMenuMultiPageViewModelBase
    {
        private const string MenuExtensionPointPath = "/Application/OperatorMenu/SystemMenu";

        public SystemMainPageViewModel(IOperatorMenuPageLoader mainPage) : base(mainPage, MenuExtensionPointPath)
        {
        }
    }
}
