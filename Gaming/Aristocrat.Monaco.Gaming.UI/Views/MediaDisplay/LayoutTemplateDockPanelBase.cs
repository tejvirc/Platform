namespace Aristocrat.Monaco.Gaming.UI.Views.MediaDisplay
{
    using Application.Contracts.Media;
    using log4net;
    using Monaco.UI.Common;
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using ViewModels;

    /// <summary>
    ///     This class is intended to maintain the different screen areas per Player User Interface Protocol V1.
    ///     The service window and banner areas will have content set by other systems.  The main content should be
    ///     directly inline in the XAML.  The logic to set and maintain sizes are controlled in the <seealso cref="LayoutTemplateViewModel"/>.
    /// </summary>
    public abstract class LayoutTemplateDockPanelBase : DockPanel, IDisposable
    {
        private enum AnimationPropertyName
        {
            Height,
            Width
        }

        public static readonly DependencyProperty ScreenTypeProperty = DependencyProperty.Register("ScreenType", typeof(ScreenType?), typeof(LayoutTemplateDockPanelBase), new PropertyMetadata(OnScreenTypeChanged));
        protected static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly Duration _duration = new Duration(new TimeSpan(0, 0, 0, 0, 750));
        private bool _disposed;

        /// <summary>
        ///     Initializes the <seealso cref="LayoutTemplateDockPanel"/>.
        /// </summary>
        protected LayoutTemplateDockPanelBase()
        {
            Loaded += OnLoaded;
            var screenTypeBinding = new Binding("ScreenType")
            {
                Source = ViewModel,
                Mode = BindingMode.OneWayToSource
            };
            SetBinding(ScreenTypeProperty, screenTypeBinding);

            var backgroundColorBinding = new Binding("BackgroundColor")
            {
                Source = ViewModel,
                Mode = BindingMode.OneWay
            };
            SetBinding(BackgroundProperty, backgroundColorBinding);

            SizeChanged += LayoutTemplateView_SizeChanged;
        }

        protected abstract ContentControl CreateView(IMediaPlayerViewModel player);

        internal LayoutTemplateViewModel ViewModel;

        /// <summary>
        ///     Gets or sets the <seealso cref="ScreenType"/> the panel is being used for.
        /// </summary>
        public ScreenType? ScreenType
        {
            get => GetValue(ScreenTypeProperty) as ScreenType?;
            set => SetValue(ScreenTypeProperty, value);
        }

        protected bool Disposed
        {
            get => _disposed;
            set => _disposed = value;
        }

        /// <summary>
        ///     Adjusts the view model's actual window width and height per any size change.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The args.</param>
        private void LayoutTemplateView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.WidthChanged)
                ViewModel.WindowWidth = ActualWidth;

            if (e.HeightChanged)
                ViewModel.WindowHeight = ActualHeight;
        }

        private static void OnScreenTypeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (!(dependencyObject is LayoutTemplateDockPanelBase panel)) return;

            panel.ViewModel.ScreenType = panel.ScreenType;

            panel.CreateControls();
        }

        /// <summary>
        ///     Creates the service window and banner controls by priority.
        /// </summary>
        private void CreateControls()
        {
            var childPosition = 0;

            ViewModel
                .MediaPlayers
                .OrderBy(p => p.Priority)
                .ToList()
                .ForEach(p =>
                {
                    Children.Insert(childPosition++, CreatePlayerContentControl(p));
                });
        }

        /// <summary>
        ///     Creates the service window content control docked to the designated location.
        /// </summary>
        /// <returns>The service window content control.</returns>
        protected ContentControl CreatePlayerContentControl(MediaPlayerViewModelBase player)
        {
            ContentControl contentControl = CreateView(player);

            contentControl.SetValue(DockProperty, player.DisplayPosition.ToDock());

            contentControl.RenderTransform = new TranslateTransform();

            // Animate here so we can use current width values
            // Cannot use a binding to width in a xaml storyboard
            player.WidthChanged += (sender, e) =>
            {
                if (!(e is ExtendedPropertyChangedEventArgs<double> args))
                {
                    return;
                }

                if (player.DisplayPosition.IsMenu())
                {
                    Storyboard sb;
                    if (player.ActualWidth > 0 && contentControl.Width > 0)
                    {
                        sb = GetResizeStoryboard(contentControl, args, AnimationPropertyName.Width);
                    }
                    else
                    {
                        // Set width before animating when width is > 0 so display is correct size to slide in
                        if (player.ActualWidth > 0)
                        {
                            contentControl.Width = player.ActualWidth;
                        }

                        sb = GetMarginStoryboard(contentControl, args, player);
                    }

                    sb.Completed += (o, a) =>
                    {
                        contentControl.Width = args.NewValue;
                    };

                    StartMediaPlayerStoryboard(sb, contentControl, player);
                }
                else
                {
                    if (args.OldValue.Equals(0))
                    {
                        return;
                    }

                    var newValue = args.NewValue;

                    // When the menu and banner are both closing, resize new banner width to window width so there won't be gaps between panels
                    if (args.OldValue > 0 && newValue.Equals(0))
                    {
                        if (args.OldValue.Equals(ViewModel.WindowWidth))
                        {
                            return;
                        }

                        args = new ExtendedPropertyChangedEventArgs<double>(null, args.OldValue, ViewModel.WindowWidth);
                    }

                    // When increasing in size, ease out will make animation slightly faster at the beginning to prevent gaps between panels
                    var mode = args.NewValue > args.OldValue ? EasingMode.EaseOut : EasingMode.EaseInOut;

                    var sb = GetResizeStoryboard(contentControl, args, AnimationPropertyName.Width, mode);

                    sb.Completed += (o, a) =>
                    {
                        contentControl.Width = player.ActualWidth > 0 ? player.ActualWidth : double.NaN;
                    };

                    sb.Begin(contentControl);
                }
            };

            player.HeightChanged += (sender, e) =>
            {
                if (!(e is ExtendedPropertyChangedEventArgs<double> args) || !player.DisplayPosition.IsBanner())
                {
                    return;
                }

                Storyboard sb;
                if (player.ActualHeight > 0 && contentControl.Height > 0)
                {
                    // Don't set IsResizing for this animation because it's due to the menu resizing
                    // It will already be set for that player and we don't want to block further banner resizing
                    sb = GetResizeStoryboard(contentControl, args, AnimationPropertyName.Height);
                    sb.Completed += (o, a) =>
                    {
                        // Wait until after storyboard has completed to clear browser so content doesn't disappear mid-animation
                        player.ClearBrowser();
                        contentControl.Height = args.NewValue;
                    };

                    sb.Begin(contentControl);
                    return;
                }

                // Set height before animating when height is > 0 so display is correct size to slide in
                if (player.ActualHeight > 0)
                {
                    contentControl.Height = player.ActualHeight;
                }

                sb = GetMarginStoryboard(contentControl, args, player);

                sb.Completed += (o, a) =>
                {
                    contentControl.Height = args.NewValue;
                };

                StartMediaPlayerStoryboard(sb, contentControl, player);
            };

            return contentControl;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            ViewModel.WindowWidth = ActualWidth;
            ViewModel.WindowHeight = ActualHeight;
        }

        private void StartMediaPlayerStoryboard(Storyboard sb, FrameworkElement contentControl, MediaPlayerViewModelBase player)
        {
            // Disable game play while animating to avoid mis-clicks
            player.IsAnimating = true;
            sb.Completed += (o, args) =>
            {
                // Wait until after storyboard has completed to clear browser so content doesn't disappear mid-animation
                player.ClearBrowser();
                player.IsAnimating = false;
            };

            sb.Begin(contentControl);
        }

        private Storyboard GetMarginStoryboard(FrameworkElement contentControl, ExtendedPropertyChangedEventArgs<double> change, MediaPlayerViewModelBase player)
        {
            // Animate margin to slide UI control into view without stretching or resizing content
            var sb = new Storyboard();
            var animation = new ThicknessAnimation
            {
                Duration = _duration,
                From = GetMarginValue(-change.NewValue, player.DisplayPosition),
                To = GetMarginValue(-change.OldValue, player.DisplayPosition),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            sb.Completed += (sender, e) =>
            {
                contentControl.Margin = GetMarginValue(-change.OldValue, player.DisplayPosition);
            };

            AddAnimationToStoryboard(sb, animation, contentControl, "Margin");

            return sb;
        }

        private Thickness GetMarginValue(double value, DisplayPosition position)
        {
            switch (position)
            {
                case DisplayPosition.Left:
                    return new Thickness(value, 0, 0, 0);
                case DisplayPosition.Top:
                    return new Thickness(0, value, 0, 0);
                case DisplayPosition.Right:
                    return new Thickness(0, 0, value, 0);
                case DisplayPosition.Bottom:
                    return new Thickness(0, 0, 0, value);
            }

            return new Thickness(0, 0, 0, 0);
        }

        private Storyboard GetResizeStoryboard(UIElement contentControl, ExtendedPropertyChangedEventArgs<double> change, AnimationPropertyName propertyName, EasingMode mode = EasingMode.EaseInOut)
        {
            var sb = new Storyboard();
            var animation = new DoubleAnimation
            {
                Duration = _duration,
                From = change.OldValue,
                To = change.NewValue,
                EasingFunction = new QuadraticEase { EasingMode = mode }
            };

            AddAnimationToStoryboard(sb, animation, contentControl, propertyName.ToString());

            return sb;
        }

        private void AddAnimationToStoryboard(Storyboard sb, AnimationTimeline animation, UIElement contentControl, string propertyName)
        {
            Timeline.SetDesiredFrameRate(sb, 120); // This sort of helps with smoothness but also slightly affects CPU usage

            Storyboard.SetTarget(animation, contentControl);
            Storyboard.SetTargetProperty(animation, new PropertyPath(propertyName));
            sb.Children.Add(animation);
            sb.FillBehavior = FillBehavior.Stop;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                ViewModel?.Dispose(); 
            }

            ViewModel = null;
            _disposed = true;
        }
    }
}
