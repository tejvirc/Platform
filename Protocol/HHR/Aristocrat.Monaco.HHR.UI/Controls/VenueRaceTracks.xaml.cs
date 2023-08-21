namespace Aristocrat.Monaco.Hhr.UI.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using log4net;
    using Models;
    using BitmapImage = System.Windows.Media.Imaging.BitmapImage;

    /// <summary>
    ///     Interaction logic for VenueRaceTracks.xaml
    /// </summary>
    public partial class VenueRaceTracks
    {
        private static readonly Dictionary<string, BitmapImage> ImageLookup = new Dictionary<string, BitmapImage>();

        private const string BackgroundTop = "Resources/VenueRaceTrackBackground_Top.PNG";
        private const string BackgroundMiddle = "Resources/VenueRaceTrackBackground_Middle.PNG";
        private const string BackgroundBottom = "Resources/VenueRaceTrackBackground_Bottom.PNG";

        private AnimationClock _clock;

        public static readonly DependencyProperty IsPausedProperty =
            DependencyProperty.RegisterAttached(
                "IsPaused",
                typeof(bool),
                typeof(VenueRaceTracks),
                new PropertyMetadata(false, IsPausedPropertyChangedCallback));

        private static void IsPausedPropertyChangedCallback(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (dependencyPropertyChangedEventArgs.NewValue != null)
            {
                var thisControl = (VenueRaceTracks)dependencyObject;

                if ((bool)dependencyPropertyChangedEventArgs.NewValue)
                {
                    thisControl.PauseAnimation();
                }
                else
                {
                    thisControl.ResumeAnimation();
                }
            }
        }

        /// <summary>
        /// Flag to pause/play the animation
        /// </summary>
        public bool IsPaused
        {
            get => (bool)GetValue(IsPausedProperty);
            set => SetValue(IsPausedProperty, value);
        }

        private void PauseAnimation()
        {
            _clock?.Controller?.Pause();
        }

        private void StopAnimation()
        {
            _clock?.Controller?.Remove();
        }

        private void ResumeAnimation()
        {
            if (_clock?.IsPaused ?? false)
            {
                _clock?.Controller?.Resume();
            }
        }

        public static readonly DependencyProperty RaceStartedProperty =
            DependencyProperty.RegisterAttached(
                "RaceStarted",
                typeof(bool),
                typeof(VenueRaceTracks),
                new PropertyMetadata(false, RaceStartedPropertyChangedCallback));

        private static void RaceStartedPropertyChangedCallback(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (dependencyPropertyChangedEventArgs.NewValue != null)
            {
                var thisControl = (VenueRaceTracks)dependencyObject;

                if ((bool)dependencyPropertyChangedEventArgs.NewValue)
                {
                    thisControl.SetupAndStartAnimation();
                }
                else
                {
                    thisControl.StopAnimation();
                }
            }
        }

        /// <summary>
        /// Flag to start the finish line animation
        /// </summary>
        public bool RaceStarted
        {
            get => (bool)GetValue(RaceStartedProperty);
            set => SetValue(RaceStartedProperty, value);
        }

        public static readonly DependencyProperty RaceFinishedProperty =
            DependencyProperty.RegisterAttached(
                "RaceFinished",
                typeof(bool),
                typeof(VenueRaceTracks),
                new PropertyMetadata(false));

        /// <summary>
        /// Flag to start the finish line animation
        /// </summary>
        public bool RaceFinished
        {
            get => (bool)GetValue(RaceFinishedProperty);
            set => SetValue(RaceFinishedProperty, value);
        }

        public static readonly DependencyProperty VenueNameProperty =
            DependencyProperty.RegisterAttached(
                "VenueName",
                typeof(string),
                typeof(VenueRaceTracks),
                new PropertyMetadata(default(string)));

        /// <summary>
        /// The name of the venue
        /// </summary>
        public string VenueName
        {
            get => (string)GetValue(VenueNameProperty);
            set => SetValue(VenueNameProperty, value);
        }

        public static readonly DependencyProperty RacingLanesProperty =
            DependencyProperty.RegisterAttached(
                "RacingLanes",
                typeof(ObservableCollection<RaceTrackEntryModel>),
                typeof(VenueRaceTracks),
                new PropertyMetadata(default(ObservableCollection<RaceTrackEntryModel>)));

        /// <summary>
        /// The collection of racing lanes where the horse will run across
        /// </summary>
        public ObservableCollection<RaceTrackEntryModel> RacingLanes
        {
            get => (ObservableCollection<RaceTrackEntryModel>)GetValue(RacingLanesProperty);
            set => SetValue(RacingLanesProperty, value);
        }

        public VenueRaceTracks()
        {
            InitializeComponent();

            if (!ImageLookup.Any())
            {
                ImageLookup[BackgroundTop] = Util.GetBitMapImage(BackgroundTop);
                ImageLookup[BackgroundMiddle] = Util.GetBitMapImage(BackgroundMiddle);
                ImageLookup[BackgroundBottom] = Util.GetBitMapImage(BackgroundBottom);
            }

            FieldTopBorder.Background = new ImageBrush
            {
                ImageSource = ImageLookup[BackgroundTop]
            };

            FieldMiddleBorder.Background = new ImageBrush
            {
                ImageSource = ImageLookup[BackgroundMiddle]
            };

            FieldBottomBorder.Background = new ImageBrush
            {
                ImageSource = ImageLookup[BackgroundBottom]
            };
        }

        private void FinishLineStoryboard_OnCompleted(object sender, EventArgs e)
        {
            RaceFinished = true;
        }

        public void SetupAndStartAnimation()
        {
            if (_clock != null)
            {
                _clock.Completed -= FinishLineStoryboard_OnCompleted;
            }

            const double distance = 376;
            TranslateTransform transform = new TranslateTransform();
            FinishLine.RenderTransform = transform;
            DoubleAnimation animation = new DoubleAnimation(
                0,
                -distance,
                TimeSpan.FromMilliseconds(UiProperties.HorseResultsRunTimeMilliseconds));
            _clock = (AnimationClock)animation.CreateClock(true);
            _clock.Completed += FinishLineStoryboard_OnCompleted;
            transform.ApplyAnimationClock(TranslateTransform.XProperty, _clock);
        }
    }
}