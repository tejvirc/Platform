namespace Aristocrat.Monaco.Gaming.UI.Views.MediaDisplay.Handlers
{
    using CefSharp;

    public class DownloadHandler : IDownloadHandler
    {

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
