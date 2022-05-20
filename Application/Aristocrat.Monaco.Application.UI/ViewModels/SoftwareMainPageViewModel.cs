namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using OperatorMenu;
    using System;

    [CLSCompliant(false)]
    public sealed class SoftwareMainPageViewModel : OperatorMenuMultiPageViewModelBase
    {
        private const string MenuExtensionPointPath = "/Application/OperatorMenu/SoftwareMenu";

        public SoftwareMainPageViewModel(string displayPageTitle) : base(displayPageTitle, MenuExtensionPointPath)
        {
        }
    }
}
