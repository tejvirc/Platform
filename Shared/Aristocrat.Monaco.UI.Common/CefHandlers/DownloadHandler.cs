namespace Aristocrat.Monaco.UI.Common.CefHandlers
{
    using System;
    using CefSharp;

    /// <inheritdoc />
    public class DownloadHandler : IDownloadHandler
    {
        /// <inheritdoc />
        public bool CanDownload(
            IWebBrowser chromiumWebBrowser,
            IBrowser browser,
            string url,
            string requestMethod)
        {
            return false;
        }



        /// <inheritdoc />
        public void OnBeforeDownload(
            IWebBrowser chromiumWebBrowser,
            IBrowser browser,
            DownloadItem downloadItem,
            IBeforeDownloadCallback callback)
        {
        }

        /// <inheritdoc />
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
