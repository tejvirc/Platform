namespace Aristocrat.Monaco.Gaming.UI.Views.MediaDisplay.Handlers
{
    using System.Reflection;
    using System.Security.Cryptography.X509Certificates;
    using System.Windows;
    using Application.Contracts.Media;
    using CefSharp;
    using Kernel;
    using log4net;

    public class RequestHandler : IRequestHandler
    {
        // Some items are handled already by the default handler
        // GetAuthCredentials default method should support the following requirements:
        // The browser environment SHOULD NOT support HTTP Authentication (RFC 7235).
        // PUI content SHOULD NOT expect that such support will be available.
        // PUI content SHOULD include content-based controls for authenticating end-users when necessary.

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IEventBus _eventBus;
        private readonly int _mediaPlayerId;

        public RequestHandler(int mediaPlayerId)
        {
            _eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
            _mediaPlayerId = mediaPlayerId;
        }

        public bool OnBeforeBrowse(
            IWebBrowser chromiumWebBrowser,
            IBrowser browser,
            IFrame frame,
            IRequest request,
            bool userGesture,
            bool isRedirect)
        {
            return false;
        }

        public void OnDocumentAvailableInMainFrame(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
        }

        public bool OnOpenUrlFromTab(
            IWebBrowser chromiumWebBrowser,
            IBrowser browser,
            IFrame frame,
            string targetUrl,
            WindowOpenDisposition targetDisposition,
            bool userGesture)
        {
            return false;
        }

        public IResourceRequestHandler GetResourceRequestHandler(
            IWebBrowser chromiumWebBrowser,
            IBrowser browser,
            IFrame frame,
            IRequest request,
            bool isNavigation,
            bool isDownload,
            string requestInitiator,
            ref bool disableDefaultHandling)
        {
            return null;
        }

        public bool GetAuthCredentials(
            IWebBrowser chromiumWebBrowser,
            IBrowser browser,
            string originUrl,
            bool isProxy,
            string host,
            int port,
            string realm,
            string scheme,
            IAuthCallback callback)
        {
            return false;
        }

        public bool OnQuotaRequest(
            IWebBrowser chromiumWebBrowser,
            IBrowser browser,
            string originUrl,
            long newSize,
            IRequestCallback callback)
        {
            return false;
        }

        public bool OnCertificateError(
            IWebBrowser chromiumWebBrowser,
            IBrowser browser,
            CefErrorCode errorCode,
            string requestUrl,
            ISslInfo sslInfo,
            IRequestCallback callback)
        {
            return false;
        }

        public bool OnSelectClientCertificate(
            IWebBrowser chromiumWebBrowser,
            IBrowser browser,
            bool isProxy,
            string host,
            int port,
            X509Certificate2Collection certificates,
            ISelectClientCertificateCallback callback)
        {
            return false;
        }

        public void OnPluginCrashed(IWebBrowser chromiumWebBrowser, IBrowser browser, string pluginPath)
        {
        }

        public void OnRenderViewReady(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
        }

        /// <summary>
        ///     Called when the render process terminates unexpectedly.
        /// </summary>
        /// <param name="browserControl">The ChromiumWebBrowser control</param>
        /// <param name="browser">the browser object</param>
        /// <param name="status">indicates how the process terminated.</param>
        /// <remarks>
        ///     Remember that <see cref="browserControl" /> is likely on a different thread so care should be used
        ///     when accessing its properties.
        /// </remarks>
        public void OnRenderProcessTerminated(IWebBrowser browserControl, IBrowser browser, CefTerminationStatus status)
        {
            // Invoke to access Address property on ChromiumWebBrowser
            Application.Current.Dispatcher.Invoke(
                () =>
                {
                    Logger.Warn(
                        $"Browser terminated with status '{status}' while at address {browserControl.Address}.");
                });

            _eventBus.Publish(new BrowserProcessTerminatedEvent(_mediaPlayerId));
        }
    }
}