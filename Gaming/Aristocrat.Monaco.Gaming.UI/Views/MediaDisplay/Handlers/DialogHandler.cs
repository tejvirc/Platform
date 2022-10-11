namespace Aristocrat.Monaco.Gaming.UI.Views.MediaDisplay.Handlers
{
    using System.Collections.Generic;
    using CefSharp;

    internal class DialogHandler : IDialogHandler
    {
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
