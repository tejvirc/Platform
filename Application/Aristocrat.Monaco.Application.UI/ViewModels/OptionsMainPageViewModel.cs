﻿namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using OperatorMenu;
    using System;

    [CLSCompliant(false)]
    public class OptionsMainPageViewModel : OperatorMenuMultiPageViewModelBase
    {
        private const string MenuExtensionPointPath = "/Application/OperatorMenu/OptionsMainMenu";

        /// <summary>
        ///     Initializes a new instance of the <see cref="OptionsMainPageViewModel" /> class.
        /// </summary>
        public OptionsMainPageViewModel(string displayPageTitle) : base(displayPageTitle, MenuExtensionPointPath)
        {
        }
    }
}
