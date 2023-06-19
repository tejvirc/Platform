namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using Contracts.OperatorMenu;
    using OperatorMenu;
    using System;

    [CLSCompliant(false)]
    public class IdentificationMainPageViewModel : OperatorMenuMultiPageViewModelBase
    {
        private const string MenuExtensionPointPath = "/Application/OperatorMenu/IdentificationMenu";

        public IdentificationMainPageViewModel(IOperatorMenuPageLoader mainPage) : base(mainPage, MenuExtensionPointPath)
        {
        }
    }
}
