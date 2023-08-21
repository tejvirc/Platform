namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using Contracts.OperatorMenu;
    using OperatorMenu;

    [CLSCompliant(false)]
    public sealed class SoftwareMainPageViewModel : OperatorMenuMultiPageViewModelBase
    {
        private const string MenuExtensionPointPath = "/Application/OperatorMenu/SoftwareMenu";

        public SoftwareMainPageViewModel(IOperatorMenuPageLoader mainPage) : base(mainPage, MenuExtensionPointPath)
        {
        }
    }
}
