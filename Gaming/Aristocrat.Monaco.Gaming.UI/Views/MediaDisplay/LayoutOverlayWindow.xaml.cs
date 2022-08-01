namespace Aristocrat.Monaco.Gaming.UI.Views.MediaDisplay
{
    using Application.Contracts.Media;
    using Kernel;
    using log4net;
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media.Animation;
    using ViewModels;
    using ViewModels.MediaDisplay;

    /// <summary>
    /// Interaction logic for LayoutOverlayWindow.xaml
    /// </summary>
    public partial class LayoutOverlayWindow
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IBrowserProcessManager _browserManager;

        public static readonly DependencyProperty ScreenTypeProperty = DependencyProperty.Register("ScreenType", typeof(ScreenType?), typeof(LayoutOverlayWindow));

        private MediaPlayerViewModel _mediaPlayer;
        
        public LayoutOverlayWindow(ScreenType screenType)
        {
            ScreenType = screenType;

            InitializeComponent();

            _browserManager = ServiceManager.GetInstance().GetService<IBrowserProcessManager>();

            ViewModel = new LayoutTemplateViewModel(screenType, DisplayType.Overlay);
            ViewModel.BrowserProcessTerminated += ViewModel_OnBrowserProcessTerminated;

            SizeChanged += LayoutOverlayWindow_SizeChanged;
            CreateControls();
        }

        private void ViewModel_OnBrowserProcessTerminated(object sender, BrowserProcessTerminatedEventArgs e)
        {
            RestartBrowser(e.Id);
        }

        /// <summary>
        ///     Gets or sets the <seealso cref="ScreenType"/> the panel is being used for.
        /// </summary>
        public ScreenType? ScreenType
        {
            get => GetValue(ScreenTypeProperty) as ScreenType?;

            set => SetValue(ScreenTypeProperty, value);
        }

        /// <summary>
        ///     Gets or sets the view model.
        /// </summary>
        public LayoutTemplateViewModel ViewModel
        {
            get => GetValue(DataContextProperty) as LayoutTemplateViewModel;

            set => SetValue(DataContextProperty, value);
        }

        public void RestartBrowser(int id)
        {
            Dispatcher.Invoke(
                () =>
                {
                    if (OverlayCanvas.Children.Count == 0 || _mediaPlayer == null)
                    {
                        return;
                    }

                    // Remove browser with MP id and restart using CreatePlayerContentControl
                    if (OverlayCanvas.Children[0] is MediaPlayerView view && _mediaPlayer.Id == id)
                    {
                        Logger.Info($"Restarting browser for ID {id}");

                        var previousVisible = _mediaPlayer.IsVisible;
                        _mediaPlayer.SetVisibility(false);
                        OverlayCanvas.Children.Remove(view);
                        _browserManager.ReleaseBrowser(view.Browser);

                        CreatePlayerContentControl(_mediaPlayer);
                        _mediaPlayer.SetVisibility(previousVisible);
                    }
                });
        }

        /// <summary>
        ///     Updates the view model window size properties after the window's size has changed
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The args.</param>
        private void LayoutOverlayWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.WidthChanged)
                ViewModel.WindowWidth = ActualWidth;

            if (e.HeightChanged)
                ViewModel.WindowHeight = ActualHeight;
        }

        /// <summary>
        ///     Creates the service window and banner controls by priority.
        /// </summary>
        private void CreateControls()
        {
            var players = ViewModel
                .MediaPlayers
                .Where(p => p.ScreenType == ScreenType)
                .OrderByDescending(p => p.Priority)
                .ToList();

            if (players.Count > 1)
            {
                Logger.Warn("CreateControls: More than one media player was found for this overlay window");
            }

            // There should only be one media player per overlay window
            _mediaPlayer = (MediaPlayerViewModel)players.FirstOrDefault();
            if (_mediaPlayer != null)
            {
                CreatePlayerContentControl(_mediaPlayer);
            }
        }

        /// <summary>
        ///     Creates the service window content control docked to the designated location.
        /// </summary>
        /// <returns>The service window content control.</returns>
        private void CreatePlayerContentControl(MediaPlayerViewModel player)
        {
            var contentControl = _browserManager.StartNewBrowser(player);

            contentControl.Visibility = Visibility.Hidden;
            contentControl.SetValue(Canvas.LeftProperty, player.ActualX);
            contentControl.SetValue(Canvas.TopProperty, player.ActualY);

            var widthBinding = new Binding("ActualWidth") { Mode = BindingMode.OneWay, Source = player };
            contentControl.SetBinding(ContentControl.WidthProperty, widthBinding);

            var heightBinding = new Binding("ActualHeight") { Mode = BindingMode.OneWay, Source = player };
            contentControl.SetBinding(ContentControl.HeightProperty, heightBinding);

            // Animate opacity on visibility changed of overlay media player
            var duration = new Duration(new TimeSpan(0, 0, 0, 0, 500));
            player.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "IsVisible")
                {
                    Dispatcher.Invoke(
                        () =>
                        {
                            if (player.IsVisible)
                            {
                                // Set visibility to visible before beginning animation
                                contentControl.Visibility = Visibility.Visible;
                                var sb = new Storyboard();
                                var animation = new DoubleAnimation
                                {
                                    Duration = duration,
                                    From = 0.0,
                                    To = 1.0
                                };

                                Storyboard.SetTarget(animation, contentControl);
                                Storyboard.SetTargetProperty(animation, new PropertyPath("Opacity"));
                                sb.Children.Add(animation);
                                sb.FillBehavior = FillBehavior.Stop;
                                player.IsAnimating = true;
                                sb.Completed += (o, eventArgs) =>
                                {
                                    player.IsAnimating = false;

                                    Mouse.Capture(this, CaptureMode.SubTree);
                                    Mouse.Capture(null);
                                };
                                sb.Begin(contentControl);
                            }
                            else
                            {
                                var sb = new Storyboard();
                                var animation = new DoubleAnimation
                                {
                                    Duration = duration,
                                    From = 1.0,
                                    To = 0.0
                                };

                                Storyboard.SetTarget(animation, contentControl);
                                Storyboard.SetTargetProperty(animation, new PropertyPath("Opacity"));
                                sb.Children.Add(animation);
                                sb.FillBehavior = FillBehavior.Stop;
                                player.IsAnimating = true;
                                // Wait until animation is finished to set visibility to hidden
                                sb.Completed += (o, eventArgs) =>
                                {
                                    contentControl.Visibility = Visibility.Hidden;
                                    player.IsAnimating = false;
                                };
                                sb.Begin(contentControl);
                            }
                        });
                }
            };

            OverlayCanvas.Children.Add(contentControl);
        }

        private void LayoutOverlayWindow_OnClosed(object sender, EventArgs e)
        {
            ViewModel.BrowserProcessTerminated -= ViewModel_OnBrowserProcessTerminated;
            ViewModel?.Dispose();

            SizeChanged -= LayoutOverlayWindow_SizeChanged;
            _browserManager.Dispose();

            ViewModel = null;
            GC.SuppressFinalize(this);
        }
    }
}
