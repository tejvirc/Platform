namespace Aristocrat.Monaco.Hhr.UI.Controls
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using System.Windows.Media.Imaging;
    using log4net;
    using MVVM;
    using WpfAnimatedGif;

    /// <summary>
    ///     Interaction logic for RaceTrackEntry.xaml
    /// </summary>
    public partial class RaceTrackEntry
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly Dictionary<int, BitmapImage> HorseNumberToImageLookup = new Dictionary<int, BitmapImage>();

        private readonly object _lock = new object();

        private bool _initialized;

        private AnimationClock _clock;

        /// <summary>
        ///     Path to the gif of the galloping horse
        /// </summary>
        public static string HorseGifPath(int horseNumber) => $"Resources\\horses\\horse-{horseNumber}.gif";

        /// <summary>
        ///     Cached image of the galloping horse
        /// </summary>
        public BitmapImage HorseImage => HorseNumberToImageLookup[HorseNumber];

        static RaceTrackEntry()
        {
            Logger.Debug($"Horse gif FPS: {HhrUiConstants.AnimationFramesPerSecond}");
        }

        public RaceTrackEntry()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Flag to pause/play the animation
        /// </summary>
        public bool IsPaused
        {
            get => (bool)GetValue(IsPausedProperty);
            set => SetValue(IsPausedProperty, value);
        }

        public static readonly DependencyProperty IsPausedProperty =
            DependencyProperty.RegisterAttached(
                "IsPaused",
                typeof(bool),
                typeof(RaceTrackEntry),
                new PropertyMetadata(false, IsPausedPropertyChangedCallback));

        private static void IsPausedPropertyChangedCallback(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (dependencyPropertyChangedEventArgs.NewValue != null)
            {
                var thisControl = (RaceTrackEntry)dependencyObject;

                if ((bool)dependencyPropertyChangedEventArgs.NewValue)
                {
                    thisControl.PauseAnimation();
                }
                else if (thisControl._initialized)
                {
                    thisControl.PlayAnimation();
                }
            }
        }

        private void PauseAnimation()
        {
            lock (_lock)
            {
                _clock?.Controller?.Pause();
                MvvmHelper.ExecuteOnUI(() =>
                {
                    ImageBehavior.GetAnimationController(Horse)?.Pause();
                });
            }
        }

        private void PlayAnimation()
        {
            lock (_lock)
            {
                if (_clock?.IsPaused ?? false)
                {
                    _clock?.Controller?.Resume();
                }

                if (_initialized)
                {
                    MvvmHelper.ExecuteOnUI(() =>
                    {
                        ImageBehavior.GetAnimationController(Horse)?.Play();
                    });
                }
            }
        }

        /// <summary>
        /// Flag to start the race animation
        /// </summary>
        public bool RaceStarted
        {
            get => (bool)GetValue(RaceStartedProperty);
            set => SetValue(RaceStartedProperty, value);
        }

        public static readonly DependencyProperty RaceStartedProperty =
            DependencyProperty.RegisterAttached(
                "RaceStarted",
                typeof(bool),
                typeof(RaceTrackEntry),
                new PropertyMetadata(false, RaceStartedPropertyChangedCallback));

        private static void RaceStartedPropertyChangedCallback(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (dependencyPropertyChangedEventArgs.NewValue != null)
            {
                if ((bool)dependencyPropertyChangedEventArgs.NewValue)
                {
                    RaceTrackEntry thisControl = (RaceTrackEntry)dependencyObject;
                    thisControl.StartRace();
                }
            }
        }

        /// <summary>
        /// The number of the horse
        /// </summary>
        public int HorseNumber
        {
            get => (int)GetValue(HorseNumberProperty);
            set => SetValue(HorseNumberProperty, value);
        }

        public static readonly DependencyProperty HorseNumberProperty =
            DependencyProperty.RegisterAttached(
                "HorseNumber",
                typeof(int),
                typeof(RaceTrackEntry),
                new PropertyMetadata(default(int)));

        /// <summary>
        ///     The winning position for the horse running in this track
        /// </summary>
        public int Place
        {
            get => (int)GetValue(PlaceProperty);
            set => SetValue(PlaceProperty, value);
        }

        public static readonly DependencyProperty PlaceProperty =
            DependencyProperty.RegisterAttached(
                "Place",
                typeof(int),
                typeof(RaceTrackEntry),
                new PropertyMetadata(default(int)));

        public static void SetupHorseImages()
        {
            for (int i = 1; i <= HhrUiConstants.MaxNumberOfHorses; i++)
            {
                var outPutDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
                var imagePath = Path.Combine(new[] { outPutDirectory, HorseGifPath(i) });
                var imageLocalPath = new Uri(imagePath).LocalPath;

                var image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(imageLocalPath);
                image.EndInit();

                RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);
                image.Freeze();
                HorseNumberToImageLookup[i] = image;
            }
        }

        private double CalculateDistance(int place)
        {
            // The amount of distance the fastest horse will cover in the race time.
            const double distanceToRun = 250;

            var runDistance = distanceToRun * ((12 - place) / 12.0) + 50;

            return runDistance;
        }

        private void SetupAndStartAnimation(double distanceToRun)
        {
            Vector offset = VisualTreeHelper.GetOffset(Horse);
            var left = offset.X;
            TranslateTransform transform = new TranslateTransform();
            Horse.RenderTransform = transform;
            DoubleAnimation animation = new DoubleAnimation(
                0,
                distanceToRun - left,
                TimeSpan.FromMilliseconds(UiProperties.HorseResultsRunTimeMilliseconds));
            _clock = (AnimationClock)animation.CreateClock(true);
            transform.ApplyAnimationClock(TranslateTransform.XProperty, _clock);
            PlayAnimation();
        }

        private void StartRace()
        {
            lock (_lock)
            {
                _initialized = true;

                SetupAndStartAnimation(CalculateDistance(Place));
            }
        }

        private void RaceTrackEntry_OnUnloaded(object sender, RoutedEventArgs e)
        {
            lock (_lock)
            {
                _initialized = false;
            }
        }
    }
}