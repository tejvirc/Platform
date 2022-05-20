namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using OperatorMenu;
    using System;

    [CLSCompliant(false)]
    public class SystemMainPageViewModel : OperatorMenuMultiPageViewModelBase
    {
        private const string MenuExtensionPointPath = "/Application/OperatorMenu/SystemMenu";

        public SystemMainPageViewModel(string displayPageTitle) : base(displayPageTitle, MenuExtensionPointPath)
        {
        }
    }
}
