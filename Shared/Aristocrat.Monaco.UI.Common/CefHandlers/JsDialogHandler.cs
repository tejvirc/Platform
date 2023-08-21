namespace Aristocrat.Monaco.UI.Common.CefHandlers
{
    using System;
    using CefSharp;

    /// <inheritdoc />
    public class JsDialogHandler : IJsDialogHandler
    {
        /// <inheritdoc />
        public bool OnJSDialog(
            IWebBrowser chromiumWebBrowser,
            IBrowser browser,
            string originUrl,
            CefJsDialogType dialogType,
            string messageText,
            string defaultPromptText,
            IJsDialogCallback callback,
            ref bool suppressMessage)
        {
            // Set suppressMessage to true and return false to suppress the message
            suppressMessage = true;
            return false;
        }

        /// <inheritdoc />
        public bool OnBeforeUnloadDialog(
            IWebBrowser chromiumWebBrowser,
            IBrowser browser,
            string messageText,
            bool isReload,
            IJsDialogCallback callback)
        {
            // Return false to use the default dialog implementation otherwise return true to handle
            return true;
        }

        /// <inheritdoc />
        public void OnResetDialogState(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
        }

        /// <inheritdoc />
        public void OnDialogClosed(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
        }
    }
}
