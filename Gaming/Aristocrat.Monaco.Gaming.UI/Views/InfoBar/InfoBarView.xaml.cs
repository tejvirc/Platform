namespace Aristocrat.Monaco.Gaming.UI.Views.InfoBar
{
    using System;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media.Animation;
    using Cabinet.Contracts;
    using ViewModels;

    /// <summary>
    ///     Interaction logic for InfoBarView.xaml
    /// </summary>
    public partial class InfoBarView
    {
        public static readonly DependencyProperty LocationProperty = DependencyProperty.Register(
            nameof(Location),
            typeof(DisplayRole),
            typeof(InfoBarView),
            new PropertyMetadata(DisplayRole.Unknown, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is InfoBarView infoBarView && infoBarView.InfoBarGrid.DataContext is InfoBarViewModel infoBarViewModel)
            {
                infoBarViewModel.DisplayTarget = (DisplayRole)Enum.ToObject(typeof(DisplayRole), e.NewValue);
            }
        }

        public DisplayRole Location
        {
            get => (DisplayRole)GetValue(LocationProperty);
            set => SetValue(LocationProperty, value);
        }

        private const double ScrollTimeInSeconds = 10;

        private DoubleAnimation _leftAnimation;
        private DoubleAnimation _centerAnimation;
        private DoubleAnimation _rightAnimation;
        private double _leftTextWidth;
        private double _centerTextWidth;
        private double _rightTextWidth;
        private DateTime _leftAnimationExpiration;
        private DateTime _centerAnimationExpiration;
        private DateTime _rightAnimationExpiration;

        public InfoBarView()
        {
            InitializeComponent();

            InfoBarGrid.DataContext = new InfoBarViewModel();

            InfoBarGrid.SizeChanged += InfoBarGrid_SizeChanged;
            LeftText.SizeChanged += LeftText_SizeChanged;
            CenterText.SizeChanged += CenterText_SizeChanged;
            RightText.SizeChanged += RightText_SizeChanged;
        }

        private void InfoBarGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            LeftCanvas.Width = LeftGrid.Width;
            LeftCanvas.Height = LeftGrid.Height;
            CenterCanvas.Width = CenterGrid.Width;
            CenterCanvas.Height = CenterGrid.Height;
            RightCanvas.Width = RightGrid.Width;
            RightCanvas.Height = RightGrid.Height;
        }

        private void LeftText_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CheckForAnimation(LeftText, LeftCanvas, ref _leftAnimation, ref _leftTextWidth, ref _leftAnimationExpiration, HorizontalAlignment.Left);
        }

        private void CenterText_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CheckForAnimation(CenterText, CenterCanvas, ref _centerAnimation, ref _centerTextWidth, ref _centerAnimationExpiration, HorizontalAlignment.Center);
        }

        private void RightText_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CheckForAnimation(RightText, RightCanvas, ref _rightAnimation, ref _rightTextWidth, ref _rightAnimationExpiration, HorizontalAlignment.Right);
        }

        private void CheckForAnimation(TextBlock textBlock, Canvas canvasParent, ref DoubleAnimation animation, ref double oldTextWidth, ref DateTime durationExpiration, HorizontalAlignment align)
        {
            // Check for previous animation.
            var alreadyAnimating = animation != null;

            // Find adjustment if text size has decreased.
            var decreaseAdjustment = alreadyAnimating ? Math.Max(oldTextWidth - textBlock.ActualWidth, 0) : 0;

            if (textBlock.Tag == null)
            {
                return;
            }

            var duration = (double)textBlock.Tag;
            if (duration == 0)
            {
                duration = ScrollTimeInSeconds;
            }

            if (decreaseAdjustment <= 0)
            {
                durationExpiration = DateTime.Now + TimeSpan.FromSeconds(duration);
            }

            // Nothing new here.
            if (textBlock.ActualWidth == 0)
            {
                animation = null;
            }

            // So much text that it needs to scroll.
            if (textBlock.ActualWidth > canvasParent.ActualWidth && durationExpiration > DateTime.Now)
            {
                animation = new DoubleAnimation
                {
                    From = alreadyAnimating ? (double)textBlock.GetValue(Canvas.LeftProperty) + decreaseAdjustment : canvasParent.ActualWidth,
                    To = -textBlock.ActualWidth,
                    RepeatBehavior = RepeatBehavior.Forever,
                    Duration = new Duration(durationExpiration - DateTime.Now)
                };
            }

            // This text fits in its space, so let it sit still.
            else
            {
                var pos = 0.0;
                switch (align)
                {
                    case HorizontalAlignment.Center:
                        pos = (canvasParent.ActualWidth - textBlock.ActualWidth) / 2;
                        break;
                    case HorizontalAlignment.Right:
                        pos = canvasParent.ActualWidth - textBlock.ActualWidth;
                        break;
                }

                animation = new DoubleAnimation
                {
                    From = pos,
                    To = pos
                };
            }

            textBlock.BeginAnimation(Canvas.LeftProperty, animation);
            oldTextWidth = textBlock.ActualWidth;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.Closing += Window_Closing;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            (DataContext as InfoBarViewModel)?.Dispose();
        }
    }
}
