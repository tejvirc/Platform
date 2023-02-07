namespace Aristocrat.Monaco.UI.Common.CefHandlers
{
    using System;
    using System.Collections.Generic;
    using CefSharp;

    /// <inheritdoc />
    [CLSCompliant(false)]
    public class DialogHandler : IDialogHandler
    {
        /// <inheritdoc />
        public bool OnFileDialog(
            IWebBrowser chromiumWebBrowser,
            IBrowser browser,
            CefFileDialogMode mode,
            CefFileDialogFlags flags,
            string title,
            string defaultFilePath,
            List<string> acceptFilters,
            int selectedAcceptFilter,
            IFileDialogCallback callback)
        {
            // Don't allow file dialogs
            callback.Cancel();
            return true;
        }
    }
}
