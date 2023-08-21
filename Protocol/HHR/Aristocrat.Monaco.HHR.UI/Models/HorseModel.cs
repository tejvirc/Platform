namespace Aristocrat.Monaco.Hhr.UI.Models
{
    using MVVM.ViewModel;

    public class HorseModel : BaseViewModel
    {
        private int _racePosition;

        private int _number;

        private bool _isCorrectPick;

        public HorseModel(int number, int position, bool isCorrectPick)
        {
            _number = number;
            _racePosition = position;
            _isCorrectPick = isCorrectPick;
        }

        /// <summary>
        /// The horse number
        /// </summary>
        public int Number
        {
            get => _number;
            set => SetProperty(ref _number, value, nameof(Number));
        }

        /// <summary>
        /// The position of the horse
        /// </summary>
        public int RacePosition
        {
            get => _racePosition;
            set => SetProperty(ref _racePosition, value, nameof(RacePosition));
        }

        /// <summary>
        /// Whether or not this horse number was picked correctly
        /// </summary>
        public bool IsCorrectPick
        {
            get => _isCorrectPick;
            set => SetProperty(ref _isCorrectPick, value, nameof(IsCorrectPick));
        }
    }
}