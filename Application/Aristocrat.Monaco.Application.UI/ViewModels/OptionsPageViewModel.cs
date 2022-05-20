namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using OperatorMenu;
    using System;

    /// <summary>
    ///     A <see cref="OptionsPageViewModel" /> contains the logic for options page.
    /// </summary>
    [CLSCompliant(false)]
    public class OptionsPageViewModel : OperatorMenuMultiPageViewModelBase
    {
        private const string MenuExtensionPointPath = "/Application/OperatorMenu/OptionsMenu";

        /// <summary>
        ///     Initializes a new instance of the <see cref="OptionsPageViewModel" /> class.
        /// </summary>
        public OptionsPageViewModel(string displayPageTitle) : base(displayPageTitle, MenuExtensionPointPath)
        {
        }
    }
}