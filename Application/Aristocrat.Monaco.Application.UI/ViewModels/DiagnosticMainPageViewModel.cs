﻿namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using OperatorMenu;

    /// <summary>
    ///     The view model for the diagnostics "main" page, the page that shows either the history
    ///     or the live values of the platform performance counters.
    /// </summary>
    [CLSCompliant(false)]
    public class DiagnosticMainPageViewModel : OperatorMenuMultiPageViewModelBase
    {

        private const string PagesExtensionPath = @"/Application/OperatorMenu/DiagnosticMenu";

        public DiagnosticMainPageViewModel(string displayPageTitle) : base(displayPageTitle, PagesExtensionPath)
        {
        }
    }
}