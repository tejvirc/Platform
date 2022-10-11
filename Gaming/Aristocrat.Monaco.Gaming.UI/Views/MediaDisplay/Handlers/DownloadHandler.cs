namespace Aristocrat.Monaco.Gaming.UI.Views.MediaDisplay.Handlers
{
    using CefSharp;

    public class DownloadHandler : IDownloadHandler
    {
        public bool CanDownload(
            IWebBrowser chromiumWebBrowser,
            IBrowser browser,
            string url,
            string requestMethod)
        {
            return false;
        }


        public void OnBeforeDownload(
            IWebBrowser chromiumWebBrowser,
            IBrowser browser,
            DownloadItem downloadItem,
            IBeforeDownloadCallback callback)
        {
        }

        public void OnDownloadUpdated(
            IWebBrowser chromiumWebBrowser,
            IBrowser browser,
            DownloadItem downloadItem,
            IDownloadItemCallback callback)
        {
            //Cancel the download
            callback.Cancel();
        }
    }
}
