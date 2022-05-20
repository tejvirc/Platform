namespace Aristocrat.Monaco.Hhr.UI.Controls
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Kernel;
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

        private readonly ISystemDisableManager _disableManager;

        private readonly object _lock = new object();

        private bool _initialized;

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

            _disableManager = ServiceManager.GetInstance().GetService<ISystemDisableManager>();
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
                else if(thisControl._initialized)
                {
                    thisControl.PlayAnimation();
                }
            }
        }

        private void PauseAnimation()
        {
            lock (_lock)
            {
                if (_initialized)
                {
                    MvvmHelper.ExecuteOnUI(() =>
                    {
                        ImageBehavior.GetAnimationController(Horse)?.Pause();
                    });
                }
            }
        }

        private void PlayAnimation()
        {
            lock (_lock)
            {
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

        public void StartRace()
        {
            lock (_lock)
            {
                _initialized = true;

                if (_disableManager.IsDisabled && !_disableManager.IsGamePlayAllowed())
                {
                    Logger.Debug("System disabled : pause and return");
                    PauseAnimation();
                }
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