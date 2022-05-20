namespace Aristocrat.Monaco.Hhr.UI.Models
{
    using System.Collections.ObjectModel;
    using MVVM.ViewModel;

    public class VenueRaceTracksModel : BaseViewModel
    {
        private bool _raceStarted;

        /// <summary>
        /// Flag to start the race
        /// </summary>
        public bool RaceStarted
        {
            get => _raceStarted;
            set
            {
                foreach (var raceLane in RaceTrackModels)
                {
                    raceLane.RaceStarted = value;
                }

                SetProperty(ref _raceStarted, value, nameof(RaceStarted));
            }
        }

        private string _venueName;

        /// <summary>
        /// The name of the race rack venue
        /// </summary>
        public string VenueName
        {
            get => _venueName;
            set => SetProperty(ref _venueName, value, nameof(VenueName));
        }

        private ObservableCollection<RaceTrackEntryModel> _raceTrackModels;

        /// <summary>
        /// The list of race track entries for this venue
        /// </summary>
        public ObservableCollection<RaceTrackEntryModel> RaceTrackModels
        {
            get => _raceTrackModels;
            set => SetProperty(ref _raceTrackModels, value, nameof(RaceTrackModels));
        }
    }
}
