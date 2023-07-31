namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using OperatorMenu;

    [CLSCompliant(false)]
    public class SystemMainPageViewModel : OperatorMenuMultiPageViewModelBase
    {
        private const string MenuExtensionPointPath = "/Application/OperatorMenu/SystemMenu";

        public SystemMainPageViewModel(string pageNameResourceKey) : base(pageNameResourceKey, MenuExtensionPointPath)
        {
        }
    }
}
