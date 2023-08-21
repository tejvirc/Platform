namespace Aristocrat.Monaco.Gaming.UI.Views.MediaDisplay.Handlers
{
    using Application.Contracts.Media;
    using CefSharp;
    using Kernel;
    using log4net;
    using System.Reflection;
    using System.Windows;
    using ViewModels;

    /// <summary>
    ///     Custom handling for CefSharp loading.
    /// </summary>
    public class LoadHandler : ILoadHandler
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEventBus _eventBus;
        private readonly IMediaProvider _mediaProvider;
        private readonly int _mediaPlayerId;
        private readonly MediaPlayerViewModel _viewModel;

        /// <summary>
        ///     Instantiates a new instance of the <see cref="LoadHandler"/> class.
        /// </summary>
        /// <param name="viewModel">Media Player View Model associated with the handler.</param>
        public LoadHandler(MediaPlayerViewModel viewModel)
        {
            _eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
            _mediaProvider = ServiceManager.GetInstance().GetService<IMediaProvider>();

            _viewModel = viewModel;
            _mediaPlayerId = viewModel.Id;
        }

        /// <summary>
        ///     Not used.
        /// </summary>
        /// <param name="browserControl">Not used.</param>
        /// <param name="frameLoadEndArgs">Not used.</param>
        public void OnFrameLoadEnd(IWebBrowser browserControl, FrameLoadEndEventArgs frameLoadEndArgs)
        {
            //Logger.LogMessage($"Browser {_mediaPlayerId}: OnFrameLoadEnd Url={frameLoadEndArgs.Url}");
        }

        /// <summary>
        ///     Not used.
        /// </summary>
        /// <param name="browserControl">Not used.</param>
        /// <param name="frameLoadStartArgs">Not used.</param>
        public void OnFrameLoadStart(IWebBrowser browserControl, FrameLoadStartEventArgs frameLoadStartArgs)
        {
            //Logger.LogMessage($"Browser {_mediaPlayerId}: OnFrameLoadStart Url={frameLoadStartArgs.Url}");
        }

        /// <summary>
        ///     If a loading error occurs, attempt to reload the page with a delay.
        /// </summary>
        /// <param name="browserControl">The browser.</param>
        /// <param name="loadErrorArgs">The args.</param>
        public void OnLoadError(IWebBrowser browserControl, LoadErrorEventArgs loadErrorArgs)
        {
            Logger.Error($"Browser {_mediaPlayerId}: OnLoadError {loadErrorArgs.ErrorCode} ({loadErrorArgs.ErrorText}) on URL {loadErrorArgs.FailedUrl}");

            if (loadErrorArgs.FailedUrl == MediaPlayerViewModel.DummyUrl)
            {
                // Don't reload on dummy URL load error
                Logger.Warn($"Browser {_mediaPlayerId}: OnLoadError ignored for Dummy URL");
                return;
            }

            //if (loadErrorArgs.ErrorCode.ToString() == "-27")
            //{
            //    // Log and ignore error "ERR_BLOCKED_BY_RESPONSE"
            //    // This error occurs because we have blocked popups and some popups may have cross-domain origins
            //    Logger.Warn($"Browser {_mediaPlayerId}: OnLoadError ignored for ERR_BLOCKED_BY_RESPONSE");
            //    return;
            //}

            var player = _mediaProvider.GetMediaPlayer(_mediaPlayerId);
            PublishErrorEvent(player.ActiveMedia, loadErrorArgs.ErrorCode);
        }

        /// <summary>
        /// </summary>
        /// <param name="browserControl">.</param>
        /// <param name="args"></param>
        public void OnLoadingStateChange(
            IWebBrowser browserControl,
            LoadingStateChangedEventArgs args)
        {
            // Use this to alert when content is ready instead of OnFrameLoadEnd according to https://github.com/cefsharp/CefSharp/wiki/General-Usage#handlers
            // OnFrameLoadEnd occurs every single time any frame loads in the webpage
            //Logger.LogMessage($"Browser {_mediaPlayerId}: OnLoadingStateChange Loading={loadingStateChangedArgs.IsLoading}");

            var player = _mediaProvider.GetMediaPlayer(_mediaPlayerId);

            // Only send content ready if the active media address matches the one that just completed loading
            Application.Current.Dispatcher.Invoke(
                () =>
                {
                    // Don't send message again if media is already in Executing or Error state or if media is null
                    if (!args.IsLoading && player?.ActiveMedia != null && player.ActiveMedia.State != MediaState.Executing && player.ActiveMedia.State != MediaState.Error)
                    {
                        Logger.Debug($"Browser {_mediaPlayerId}: OnLoadingStateChange sending ContentReady event");
                        _eventBus.Publish(
                            new MediaPlayerContentReadyEvent(
                                _mediaPlayerId,
                                player.ActiveMedia,
                                MediaContentError.None));
                    }
                    _viewModel.SetMuted();
                });
        }

        private void PublishErrorEvent(IMedia media, CefErrorCode code)
        {
            // Do not send ContentReady with Error if already in Error state
            if (media == null || media.IsFinalized)
            {
                return;
            }

            MediaContentError error;

            switch (code)
            {
                case CefErrorCode.None:
                    error = MediaContentError.None;
                    break;
                case CefErrorCode.FileTooBig:
                case CefErrorCode.FileNoSpace:
                case CefErrorCode.MsgTooBig:
                    error = MediaContentError.FileSizeLimitation;
                    break;
                case CefErrorCode.AddressInUse:
                case CefErrorCode.AddressInvalid:
                case CefErrorCode.AddressUnreachable:
                case CefErrorCode.BlockedByAdministrator:
                case CefErrorCode.BlockedByClient:
                case CefErrorCode.ConnectionAborted:
                case CefErrorCode.ConnectionClosed:
                case CefErrorCode.ConnectionFailed:
                case CefErrorCode.ConnectionRefused:
                case CefErrorCode.ConnectionReset:
                case CefErrorCode.ConnectionTimedOut:
                case CefErrorCode.DisallowedUrlScheme:
                case CefErrorCode.InvalidUrl:
                case CefErrorCode.NameNotResolved:
                case CefErrorCode.NameResolutionFailed:
                case CefErrorCode.TooManyRedirects:
                case CefErrorCode.UnknownUrlScheme:
                case CefErrorCode.UnsafePort:
                case CefErrorCode.UnsafeRedirect:
                    error = MediaContentError.TransferFailed;
                    break;
                default: // generic runtime error for all other errors
                    error = MediaContentError.RuntimeError;
                    break;
            }

            Logger.Error($"Browser {_mediaPlayerId}: OnLoadError sending ContentReady event with {error}");
            _eventBus.Publish(new MediaPlayerContentReadyEvent(_mediaPlayerId, media, error));
        }
    }
}
