namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using Contracts.OperatorMenu;
    using OperatorMenu;

    [CLSCompliant(false)]
    public class OptionsMainPageViewModel : OperatorMenuMultiPageViewModelBase
    {
        private const string MenuExtensionPointPath = "/Application/OperatorMenu/OptionsMainMenu";

        /// <summary>
        ///     Initializes a new instance of the <see cref="OptionsMainPageViewModel" /> class.
        /// </summary>
        public OptionsMainPageViewModel(IOperatorMenuPageLoader mainPage) : base(mainPage, MenuExtensionPointPath)
        {
        }
    }
}
