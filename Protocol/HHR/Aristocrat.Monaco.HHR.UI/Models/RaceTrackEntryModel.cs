namespace Aristocrat.Monaco.Hhr.UI.Models
{
    using System.Windows;

    public class RaceTrackEntryModel : BaseViewModel
    {
        private bool _raceStarted;

        /// <summary>
        /// Flag to start the race
        /// </summary>
        public bool RaceStarted
        {
            get => _raceStarted;
            set => SetProperty(ref _raceStarted, value);
        }
        
        private int _position;

        /// <summary>
        /// The position of the race track in parents race track entry list
        /// </summary>
        public int Position
        {
            get => _position;
            set => SetProperty(ref _position, value);
        }

        private int _finishPosition;

        /// <summary>
        /// The position of the race track in parents race track entry list
        /// </summary>
        public int FinishPosition
        {
            get => _finishPosition;
            set => SetProperty(ref _finishPosition, value);
        }

        private Visibility _visibility;

        /// <summary>
        /// The visibility of the horse image
        /// </summary>
        public Visibility Visibility
        {
            get => _visibility;
            set => SetProperty(ref _visibility, value);
        }
    }
}
