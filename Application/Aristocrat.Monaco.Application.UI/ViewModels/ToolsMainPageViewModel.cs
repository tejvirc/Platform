
namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using Application.Contracts.OperatorMenu;
    using OperatorMenu;

    /// <summary>
    ///     Implements Business Logic for Tools Main Page.
    /// </summary>
    [CLSCompliant(false)]
    public class ToolsMainPageViewModel : OperatorMenuMultiPageViewModelBase
    {
        private const string MenuExtensionPointPath = "/Application/OperatorMenu/ToolsMenu";

        public ToolsMainPageViewModel(IOperatorMenuPageLoader mainPage)
            : base(mainPage, MenuExtensionPointPath)
        {
        }
    }
}
