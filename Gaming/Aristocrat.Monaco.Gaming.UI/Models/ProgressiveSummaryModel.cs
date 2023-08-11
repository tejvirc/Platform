namespace Aristocrat.Monaco.Gaming.UI.Models
{
    using CommunityToolkit.Mvvm.ComponentModel;

    /// <summary>
    ///     Progressive summary model class
    /// </summary>
    public class ProgressiveSummaryModel : ObservableObject
    {
        private string _progressiveLevel;
        private string _currentValue;
        private string _configuredGame;
        private string _winLevel;

        /// <summary>
        ///     Gets or sets the progressive level.
        /// </summary>
        public string ProgressiveLevel
        {
            get => _progressiveLevel;
            set
            {
                _progressiveLevel = value;
                OnPropertyChanged(nameof(ProgressiveLevel));
            }
        }

        /// <summary>
        ///     Gets or sets the current value.
        /// </summary>
        public string CurrentValue
        {
            get => _currentValue;
            set
            {
                _currentValue = value;
                OnPropertyChanged(nameof(CurrentValue));
            }
        }

        /// <summary>
        ///     Gets or sets the configured game.
        /// </summary>
        public string ConfiguredGame
        {
            get => _configuredGame;
            set
            {
                _configuredGame = value;
                OnPropertyChanged(nameof(ConfiguredGame));
            }
        }

        /// <summary>
        ///     Gets or sets the win level.
        /// </summary>
        public string WinLevel
        {
            get => _winLevel;
            set
            {
                _winLevel = value;
                OnPropertyChanged(nameof(WinLevel));
            }
        }
    }
}