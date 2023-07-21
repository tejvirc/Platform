namespace Aristocrat.Monaco.Gaming.UI.Views.MediaDisplay
{
    using System.Windows.Input;
    using Application.Contracts.Media;
    using Aristocrat.Toolkit.Mvvm.Extensions;
    using CefSharp;
    using Contracts.Events;
    using Handlers;
    using Kernel;
    using Monaco.UI.Common.CefHandlers;
    using ViewModels;

    /// <summary>
    /// Interaction logic for MediaPlayerView.xaml
    /// </summary>
    public partial class MediaPlayerView
    {
#if DEBUG
        private bool _devToolsVisible;
#endif

        /// <summary>
        ///     Instantiates a new instance of the <see cref="MediaPlayerView"/> class.
        /// </summary>
        public MediaPlayerView(IMediaPlayerViewModel viewModel, IEventBus eventBus)
        {
            InitializeComponent();

            DataContext = viewModel;
            Browser.DialogHandler = new DialogHandler();
            Browser.DownloadHandler = new DownloadHandler();
            Browser.DragHandler = new DragHandler();
            Browser.JsDialogHandler = new JsDialogHandler();
            Browser.LifeSpanHandler = new LifeSpanHandler();
            Browser.LoadHandler = new LoadHandler(viewModel as MediaPlayerViewModel);
            Browser.MenuHandler = new DisabledContextMenuHandler();
            Browser.RequestHandler = new RequestHandler(viewModel.Id);

            eventBus.Subscribe<MediaPlayerSetAudioMutedEvent>(this, MuteAudioHandler);

            KeyDown += OnKeyDown;
        }

        private void MuteAudioHandler(MediaPlayerSetAudioMutedEvent e)
        {
            Execute.OnUIThread(
                () =>
                {
                    if (Browser.IsBrowserInitialized)
                    {
                        Browser.GetBrowserHost().SetAudioMuted(e.Mute);
                    }
                });
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
#if DEBUG
            if (e.Key != Key.F12)
            {
                return;
            }

            if (_devToolsVisible)
            {
                Browser.CloseDevTools();
                _devToolsVisible = false;
            }
            else
            {
                Browser.ShowDevTools();
                _devToolsVisible = true;
            }
#endif
        }
    }
}
