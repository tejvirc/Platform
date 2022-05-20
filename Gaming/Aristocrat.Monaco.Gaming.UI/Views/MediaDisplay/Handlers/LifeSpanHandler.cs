namespace Aristocrat.Monaco.Gaming.UI.Views.MediaDisplay.Handlers
{
    using CefSharp;

    public class LifeSpanHandler : ILifeSpanHandler
    {
        public bool OnBeforePopup(
            IWebBrowser browserControl,
            IBrowser browser,
            IFrame frame,
            string targetUrl,
            string targetFrameName,
            WindowOpenDisposition targetDisposition,
            bool userGesture,
            IPopupFeatures popupFeatures,
            IWindowInfo windowInfo,
            IBrowserSettings browserSettings,
            ref bool noJavascriptAccess,
            out IWebBrowser newBrowser)
        {
            // Set newBrowser to null unless attempting to host the popup in a new instance of ChromiumWebBrowser
            newBrowser = null;

            // Return true to cancel the popup creation
            return true; 
        }

        public void OnAfterCreated(IWebBrowser browserControl, IBrowser browser)
        {
            
        }

        public bool DoClose(IWebBrowser browserControl, IBrowser browser)
        {
            return false;
        }

        public void OnBeforeClose(IWebBrowser browserControl, IBrowser browser)
        {

        }
    }
}
