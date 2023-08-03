namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using OperatorMenu;

    [CLSCompliant(false)]
    public sealed class SoftwareMainPageViewModel : OperatorMenuMultiPageViewModelBase
    {
        private const string MenuExtensionPointPath = "/Application/OperatorMenu/SoftwareMenu";

        public SoftwareMainPageViewModel(string pageNameResourceKey) : base(pageNameResourceKey, MenuExtensionPointPath)
        {
        }
    }
}
