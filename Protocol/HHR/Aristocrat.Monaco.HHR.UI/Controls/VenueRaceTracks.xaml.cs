namespace Aristocrat.Monaco.Hhr.UI.Controls
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;
    using Models;
    using BitmapImage=System.Windows.Media.Imaging.BitmapImage;

    /// <summary>
    ///     Interaction logic for VenueRaceTracks.xaml
    /// </summary>
    public partial class VenueRaceTracks
    {
        private static readonly Dictionary<string, BitmapImage> ImageLookup = new Dictionary<string, BitmapImage>();

        private const string BackgroundTop = "Resources/VenueRaceTrackBackground_Top.PNG";
        private const string BackgroundMiddle = "Resources/VenueRaceTrackBackground_Middle.PNG";
        private const string BackgroundBottom = "Resources/VenueRaceTrackBackground_Bottom.PNG";

        public static readonly DependencyProperty IsPausedProperty =
            DependencyProperty.RegisterAttached(
                "IsPaused",
                typeof(bool),
                typeof(VenueRaceTracks),
                new PropertyMetadata(false));

        /// <summary>
        /// Flag to pause/play the animation
        /// </summary>
        public bool IsPaused
        {
            get => (bool)GetValue(IsPausedProperty);
            set => SetValue(IsPausedProperty, value);
        }

        public static readonly DependencyProperty RaceStartedProperty =
            DependencyProperty.RegisterAttached(
                "RaceStarted",
                typeof(bool),
                typeof(VenueRaceTracks),
                new PropertyMetadata(false));

        /// <summary>
        /// Flag to start the finish line animation
        /// </summary>
        public bool RaceStarted
        {
            get => (bool)GetValue(RaceStartedProperty);
            set => SetValue(RaceStartedProperty, value);
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
    }
}