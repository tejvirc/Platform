namespace Aristocrat.Monaco.Gaming.UI.Settings
{
    using System.Collections.Generic;
    using Contracts;
    using Aristocrat.Toolkit.Mvvm.Extensions;
    using CommunityToolkit.Mvvm.Input;

    /// <summary>
    ///     Attract game settings.
    /// </summary>
    internal class GameAttractSettings : BaseObservableObject
    {
        private bool _overallAttractEnabled;
        private bool _slotAttractSelected;
        private bool _kenoAttractSelected;
        private bool _pokerAttractSelected;
        private bool _blackjackAttractSelected;
        private bool _rouletteAttractSelected;
        private bool _defaultSequenceOverriden;

        /// <summary>
        ///     Gets or sets the a value that indicates whether overall attract is enabled or not.
        /// </summary>
        public bool OverallAttractEnabled
        {
            get => _overallAttractEnabled;

            set => SetProperty(ref _overallAttractEnabled, value);
        }

        /// <summary>
        ///     Gets or sets the a value that indicates whether slot (games) selected for attract
        /// </summary>
        public bool SlotAttractSelected
        {
            get => _slotAttractSelected;

            set => SetProperty(ref _slotAttractSelected, value);
        }

        /// <summary>
        ///     Gets or sets the a value that indicates whether keno (games) selected for attract
        /// </summary>
        public bool KenoAttractSelected
        {
            get => _kenoAttractSelected;

            set => SetProperty(ref _kenoAttractSelected, value);
        }

        /// <summary>
        ///     Gets or sets the a value that indicates whether poker (games) selected for attract
        /// </summary>
        public bool PokerAttractSelected
        {
            get => _pokerAttractSelected;

            set => SetProperty(ref _pokerAttractSelected, value);
        }

        /// <summary>
        ///     Gets or sets the a value that indicates whether blackjack (games) selected for attract
        /// </summary>
        public bool BlackjackAttractSelected
        {
            get => _blackjackAttractSelected;

            set => SetProperty(ref _blackjackAttractSelected, value);
        }

        /// <summary>
        ///     Gets or sets the a value that indicates whether Roulette (games) selected for attract
        /// </summary>
        public bool RouletteAttractSelected
        {
            get => _rouletteAttractSelected;

            set => SetProperty(ref _rouletteAttractSelected, value);
        }

        /// <summary>
        ///     Gets or sets the a value that indicates whether default attract is overriden.
        /// </summary>
        public bool DefaultSequenceOverriden
        {
            get => _defaultSequenceOverriden;

            set => SetProperty(ref _defaultSequenceOverriden, value);
        }

        public IEnumerable<IAttractInfo> AttractSequence { get; set; }
    }
}
