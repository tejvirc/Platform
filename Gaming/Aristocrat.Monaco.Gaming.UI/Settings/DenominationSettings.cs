namespace Aristocrat.Monaco.Gaming.UI.Settings
{
    using CommunityToolkit.Mvvm.ComponentModel;

    /// <summary>
    ///     Denomination settings.
    /// </summary>
    internal class DenominationSettings : ObservableObject
    {
        private long _id;
        private long _value;
        private bool _active;
        private int _minimumWagerCredits;
        private int _maximumWagerCredits;
        private int _maximumWagerOutsideCredits;
        private string _betOption;
        private string _lineOption;
        private int _bonusBet;
        private bool _secondaryAllowed;
        private bool _letItRideAllowed;
        private bool _betKeeperAllowed;

        /// <summary>
        ///     Gets or sets the identifier of the denomination.
        /// </summary>
        public long Id
        {
            get => _id;

            set => SetProperty(ref _id, value);
        }

        /// <summary>
        ///     Gets the value of each credit wagered as part of the game
        /// </summary>
        public long Value
        {
            get => _value;

            set => SetProperty(ref _value, value);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether or not the denomination is active
        /// </summary>
        public bool Active
        {
            get => _active;

            set => SetProperty(ref _active, value);
        }

        /// <summary>
        ///     Gets or sets the minimum wager
        /// </summary>
        public int MinimumWagerCredits
        {
            get => _minimumWagerCredits;

            set => SetProperty(ref _minimumWagerCredits, value);
        }

        /// <summary>
        ///     Gets or sets the maximum wager
        /// </summary>
        public int MaximumWagerCredits
        {
            get => _maximumWagerCredits;

            set => SetProperty(ref _maximumWagerCredits, value);
        }

        /// <summary>
        ///     Gets or sets the maximum wager for low-odds bets, for example betting in roulette on just red/black or odd/even
        /// </summary>
        public int MaximumWagerOutsideCredits
        {
            get => _maximumWagerOutsideCredits;

            set => SetProperty(ref _maximumWagerOutsideCredits, value);
        }

        /// <summary>
        ///     Gets or sets the Bet Option
        /// </summary>
        public string BetOption
        {
            get => _betOption;

            set => SetProperty(ref _betOption, value);
        }

        /// <summary>
        ///     Gets or sets the Bet Option
        /// </summary>
        public string LineOption
        {
            get => _lineOption;

            set => SetProperty(ref _lineOption, value);
        }

        /// <summary>
        ///     Gets or sets the Bonus bet
        /// </summary>
        public int BonusBet
        {
            get => _bonusBet;

            set => SetProperty(ref _bonusBet, value);
        }

        /// <summary>
        ///     Gets or sets whether or not the gamble feature is allowed
        /// </summary>
        public bool SecondaryAllowed
        {
            get => _secondaryAllowed;

            set => SetProperty(ref _secondaryAllowed, value);
        }

        /// <summary>
        ///     Gets or sets whether or not the let it ride feature is allowed
        /// </summary>
        public bool LetItRideAllowed
        {
            get => _letItRideAllowed;

            set => SetProperty(ref _letItRideAllowed, value);
        }

        /// <summary>
        ///     Gets or sets whether or not the bet keeper feature is allowed
        /// </summary>
        public bool BetKeeperAllowed
        {
            get => _betKeeperAllowed;

            set => SetProperty(ref _betKeeperAllowed, value);
        }

        /// <summary>
        ///     Performs conversion from <see cref="Denomination"/> to <see cref="DenominationSettings"/>.
        /// </summary>
        /// <param name="denom">The <see cref="Denomination"/> object.</param>
        public static explicit operator DenominationSettings(Denomination denom) => new DenominationSettings
        {
            Id = denom.Id,
            Value = denom.Value,
            BetOption = denom.BetOption,
            LineOption = denom.LineOption,
            BonusBet = denom.BonusBet,
            MinimumWagerCredits = denom.MinimumWagerCredits,
            MaximumWagerCredits = denom.MaximumWagerCredits,
            MaximumWagerOutsideCredits = denom.MaximumWagerOutsideCredits,
            SecondaryAllowed = denom.SecondaryAllowed,
            LetItRideAllowed = denom.LetItRideAllowed,
            BetKeeperAllowed = denom.BetKeeperAllowed,
            Active = denom.Active
        };

        /// <summary>
        ///     Performs conversion from <see cref="DenominationSettings"/> to <see cref="Denomination"/>.
        /// </summary>
        /// <param name="settings">The <see cref="DenominationSettings"/> setting.</param>
        public static explicit operator Denomination(DenominationSettings settings) => new Denomination
        {
            Id = settings.Id,
            Value = settings.Value,
            BetOption = settings.BetOption,
            BonusBet = settings.BonusBet,
            MinimumWagerCredits = settings.MinimumWagerCredits,
            MaximumWagerCredits = settings.MaximumWagerCredits,
            MaximumWagerOutsideCredits = settings.MaximumWagerOutsideCredits,
            SecondaryAllowed = settings.SecondaryAllowed,
            LetItRideAllowed = settings.LetItRideAllowed,
            BetKeeperAllowed = settings.BetKeeperAllowed,
            Active = settings.Active
        };
    }
}
