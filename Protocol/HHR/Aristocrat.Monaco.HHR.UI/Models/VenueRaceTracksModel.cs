namespace Aristocrat.Monaco.Hhr.UI.Models
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    public class VenueRaceTracksModel : BaseObservableObject
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

                SetProperty(ref _raceStarted, value);
            }
        }

        public EventHandler RaceFinishedEventHandler;

        private bool _raceFinished;

        /// <summary>
        /// Flag indicating the race is finished
        /// </summary>
        public bool RaceFinished
        {
            get => _raceFinished;
            set
            {
                SetProperty(ref _raceFinished, value);
                RaceFinishedEventHandler.Invoke(this, new PropertyChangedEventArgs(nameof(RaceFinished)));
            }
        }

        private string _venueName;

        /// <summary>
        /// The name of the race rack venue
        /// </summary>
        public string VenueName
        {
            get => _venueName;
            set => SetProperty(ref _venueName, value);
        }

        private ObservableCollection<RaceTrackEntryModel> _raceTrackModels;

        /// <summary>
        /// The list of race track entries for this venue
        /// </summary>
        public ObservableCollection<RaceTrackEntryModel> RaceTrackModels
        {
            get => _raceTrackModels;
            set => SetProperty(ref _raceTrackModels, value);
        }
    }
}
