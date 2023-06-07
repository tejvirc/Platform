﻿namespace Aristocrat.Monaco.UI.Common.CefHandlers
{
    using System;
    using System.Collections.Generic;
    using CefSharp;

    /// <inheritdoc />
    public class DialogHandler : IDialogHandler
    {
        /// <inheritdoc />
        public bool OnFileDialog(
            IWebBrowser chromiumWebBrowser,
            IBrowser browser,
            CefFileDialogMode mode,
            string title,
            string defaultFilePath,
            List<string> acceptFilters,
            IFileDialogCallback callback)
        {
            // Don't allow file dialogs
            callback.Cancel();
            return true;
        }
    }
}
