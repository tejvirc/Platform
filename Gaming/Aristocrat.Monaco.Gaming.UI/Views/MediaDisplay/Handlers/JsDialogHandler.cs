namespace Aristocrat.Monaco.Gaming.UI.Views.MediaDisplay.Handlers
{
    using CefSharp;

    public class JsDialogHandler : IJsDialogHandler
    {
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

        public void OnResetDialogState(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
        }

        public void OnDialogClosed(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
        }
    }
}
