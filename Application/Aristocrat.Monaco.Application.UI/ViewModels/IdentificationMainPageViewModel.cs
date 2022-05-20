namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using OperatorMenu;
    using System;

    [CLSCompliant(false)]
    public class IdentificationMainPageViewModel : OperatorMenuMultiPageViewModelBase
    {
        private const string MenuExtensionPointPath = "/Application/OperatorMenu/IdentificationMenu";

        public IdentificationMainPageViewModel(string displayPageTitle) : base(displayPageTitle, MenuExtensionPointPath)
        {
        }
    }
}
