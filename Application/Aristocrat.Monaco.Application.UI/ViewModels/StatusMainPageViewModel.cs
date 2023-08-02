namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using OperatorMenu;

    [CLSCompliant(false)]
    public class StatusMainPageViewModel : OperatorMenuMultiPageViewModelBase
    {
        private const string MenuExtensionPointPath = "/Application/OperatorMenu/StatusMenu";

        public StatusMainPageViewModel(string pageNameResourceKey) : base(pageNameResourceKey, MenuExtensionPointPath)
        {
        }
    }
}
