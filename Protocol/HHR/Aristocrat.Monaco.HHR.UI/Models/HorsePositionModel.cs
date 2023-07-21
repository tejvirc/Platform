namespace Aristocrat.Monaco.Hhr.UI.Models
{
    using Aristocrat.Toolkit.Mvvm.Extensions;

    public class HorsePositionModel : BaseObservableObject
    {
        private HorseModel _horseInfo;

        public HorsePositionModel(int position)
        {
            HorsePosition = position;
        }

        /// <summary>
        ///  The horse model object for this position
        /// </summary>
        public HorseModel HorseInfo
        {
            get => _horseInfo;
            set
            {
                SetProperty(ref _horseInfo, value, nameof(HorseInfo));
                OnPropertyChanged(nameof(PositionSelected));
                OnPropertyChanged(nameof(HorseNumber));
            }
        }

        /// <summary>
        /// The position of the horse
        /// </summary>
        public int HorsePosition { get; }

        /// <summary>
        /// True if this position has been set, false otherwise
        /// </summary>
        public bool PositionSelected => HorseInfo?.RacePosition > 0;

        /// <summary>
        /// The horse number
        /// </summary>
        public int HorseNumber => HorseInfo?.Number ?? 0;
    }
}
