namespace Aristocrat.Monaco.Gaming.UI.Views.MediaDisplay
{
    using Application.Contracts.Media;
    using Kernel;
    using System.Windows.Controls;
    using System.Windows.Media;
    using ViewModels;

    /// <summary>
    ///     This class is intended to maintain the different screen areas per Player User Interface Protocol V1.
    ///     The service window and banner areas will have content set by other systems.  The main content should be
    ///     directly inline in the XAML.  The logic to set and maintain sizes are controlled in the <seealso cref="LayoutTemplateViewModel"/>.
    /// </summary>
    public class LayoutTemplateDockPanel : LayoutTemplateDockPanelBase
    {
        private readonly IBrowserProcessManager _browserManager; 

        /// <summary>
        ///  Initializes the <seealso cref="LayoutTemplateDockPanel"/>.
        /// </summary>
        public LayoutTemplateDockPanel()
        {
            _browserManager = ServiceManager.GetInstance().GetService<IBrowserProcessManager>();
            ViewModel = new LayoutTemplateViewModel(ScreenType, DisplayType.Scale);
            ViewModel.BrowserProcessTerminated += (sender, i) => RestartBrowser(i);
            Background = Brushes.Black;
            // This is necessary to prevent dragging the window around which happens when Background is Transparent
        }

        public void RestartBrowser(int id)
        {
            Dispatcher.Invoke(
                () =>
                {
                    // Remove browser with MP id and restart using CreatePlayerContentControl
                    MediaPlayerView view = null;
                    MediaPlayerViewModel viewModel = null;
                    var enumerator = Children.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        if (enumerator.Current is MediaPlayerView v && v.DataContext is MediaPlayerViewModel vm && vm.Id == id)
                        {
                            view = v;
                            viewModel = vm;
                            break;
                        }
                    }

                    if (view != null)
                    {
                        Logger.Info($"Restarting browser for ID {id}");

                        var previousVisible = viewModel!.IsVisible;
                        viewModel.SetVisibility(false);
                        var index = Children.IndexOf(view);
                        Children.Remove(view);
                        _browserManager.ReleaseBrowser(view.Browser);

                        // Reinsert into the same position so it docks correctly
                        Children.Insert(index, CreatePlayerContentControl(viewModel));
                        viewModel.SetVisibility(previousVisible);
                    }
                });
        }

        protected override ContentControl CreateView(IMediaPlayerViewModel player)
        {
            return _browserManager.StartNewBrowser(player);
        }

        protected override void Dispose(bool disposing)
        {
            if (Disposed)
                return;

            if (disposing)
            {
                _browserManager?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
