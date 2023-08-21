namespace Aristocrat.Monaco.PackageManifest.Models
{
    using System.Collections.Generic;
    using Aristocrat.PackageManifest.Extension.v100;

    /// <summary>
    ///     The game attributes as defined in the manifest.
    /// </summary>
    public class GameAttributes
    {
        /// <summary>
        ///     Gets or sets the identifier of the theme.
        /// </summary>
        /// <value>Theme of the game, such as red-white-blue, super sevens, and so on.</value>
        public string ThemeId { get; set; }

        /// <summary>
        ///     Gets or sets the identifier of the paytable.
        /// </summary>
        /// <value>Algorithms used to determine the payouts from the game.</value>
        public string PaytableId { get; set; }

        /// <summary>
        ///     Gets or sets the maximum payback pct.
        /// </summary>
        /// <value>
        ///     Maximum theoretical payback percentage for the game; a value of 0 (zero) indicates that the information is not
        ///     available; otherwise, it MUST be set to the maximum payback percentage of the game, which MUST be greater than 0
        ///     (zero). For example, a value of 96371 represents a maximum payback percentage of 96.371%.
        /// </value>
        public long MaxPaybackPercent { get; set; }

        /// <summary>
        ///     Gets or sets the minimum payback pct.
        /// </summary>
        /// <value>
        ///     Minimum theoretical payback percentage for the game; a value of 0 (zero) indicates that the information is not
        ///     available; otherwise, it MUST be set to the minimum payback percentage for the game, which MUST be greater than 0
        ///     (zero) and less than or equal to maxPaybackPct. For example, a value of 82451 represents a minimum payback
        ///     percentage of 82.451%.
        /// </value>
        public long MinPaybackPercent { get; set; }

        /// <summary>
        ///     Gets or sets the meter name used to retrieve and display the current bonus or progressive value in the lobby or
        ///     anywhere in the UI.
        /// </summary>
        public string DisplayMeterName { get; set; }

        /// <summary>
        ///     Configures the meter names the platform should use when displaying the associated sap progressive pools in the lobby
        /// </summary>
        public string AssociatedSapDisplayMeterName { get; set; }

        /// <summary>
        ///     Gets or sets the initial value for a bonus or progressive game, which is used in the lobby or anywhere in the UI.
        ///     Typically only used until there has been at least one game round, since the game meter values trump this value.
        /// </summary>
        public long InitialValue { get; set; }

        /// <summary>
        ///     Gets or sets the flag indicating whether the game allows a secondary game.
        /// </summary>
        /// <value>Indicates whether the game can wager the game winnings through a secondary game, such as double-or-nothing.</value>
        public bool SecondaryAllowed { get; set; }

        /// <summary>
        ///     Gets or sets the flag indicating whether the game supports secondary games.
        /// </summary>
        /// <value>Indicates whether the game can wager the game winnings through a secondary game, such as double-or-nothing.</value>
        public bool SecondaryEnabled { get; set; }

        /// <summary>
        ///     Gets or sets the flag indicating whether the game supports the let it ride feature.
        /// </summary>
        /// <value>Indicates whether the game can apply the win amount as the bet amount.</value>
        public bool LetItRideAllowed { get; set; }

        /// <summary>
        ///     Gets or sets the flag indicating whether the game supports Let It Ride games.
        /// </summary>
        /// <value>Indicates whether the game can apply the win amount as the bet amount.</value>
        public bool LetItRideEnabled { get; set; }

        /// <summary>
        ///     Gets or sets the denominations as values in millicents.
        /// </summary>
        /// <value>
        ///     The available denominations.
        /// </value>
        public IEnumerable<long> Denominations { get; set; }

        /// <summary>
        ///     Gets or sets the wager categories.
        /// </summary>
        public IEnumerable<WagerCategory> WagerCategories { get; set; }

        /// <summary>
        ///     Gets or sets the CDS theme id
        /// </summary>
        public string CdsThemeId { get; set; }

        /// <summary>
        ///     Gets or sets the CDS title id
        /// </summary>
        public string CdsTitleId { get; set; }

        /// <summary>
        ///     Gets or sets the central information
        /// </summary>
        public IEnumerable<CentralInfo> CentralInfo { get; set; }

        /// <summary>
        ///     Gets ths internal variation Id used to identify the maths for a game
        /// </summary>
        public string VariationId { get; set; }

        /// <summary>
        ///     Gets or sets the type of the game.
        /// </summary>
        public t_gameType GameType { get; set; }

        /// <summary>
        ///     Gets or sets the sub type of the game.
        /// </summary>
        public string GameSubtype { get; set; }

        /// <summary>
        ///     Gets or sets the Bet Options.
        /// </summary>
        public BetOptionList BetOptionList { get; set; }

        /// <summary>
        ///     Gets or sets the Line Options.
        /// </summary>
        public LineOptionList LineOptionList { get; set; }

        /// <summary>
        ///     Gets or sets the BetLinePresets.
        /// </summary>
        public BetLinePresetList BetLinePresetList { get; set; }

        /// <summary>
        ///     Gets or sets the ActiveBetOption.
        /// </summary>
        public BetOption ActiveBetOption { get; set; }

        /// <summary>
        ///     Gets or sets the ActiveLineOption.
        /// </summary>
        public LineOption ActiveLineOption { get; set; }

        /// <summary>
        ///     Gets or sets the Win Threshold.
        /// </summary>
        public long WinThreshold { get; set; }

        /// <summary>
        ///     Gets or sets the Max Progressives per Denomination.
        /// </summary>
        public int? MaximumProgressivePerDenom { get; set; }

        /// <summary>
        ///     Gets or sets the Reference Id.
        /// </summary>
        public string ReferenceId { get; set; }

        /// <summary>
        ///     Gets or sets the game category.
        /// </summary>
        public t_category? Category { get; set; }

        /// <summary>
        ///     Gets or sets the game subcategory.
        /// </summary>
        public t_subCategory? SubCategory { get; set; }

        /// <summary>
        ///     Gets or sets the supported game's feature like BetKeeper/LuckyChanceSpin
        /// </summary>
        public IEnumerable<Feature> Features { get; set; }

        /// <summary>
        ///    Gets or sets the maximum wager for higher-odd bets, for example, betting on a specific number in roulette
        /// </summary>
        public int MaxWagerInsideCredits { get; set; }

        /// <summary>
        ///    Gets or sets the maximum wager for lower-odd bets, for example, betting on red/black or even/odd in roulette 
        /// </summary>
        public int MaxWagerOutsideCredits { get; set; }

        /// <summary>
        ///     Specifies that a game uses the next-to-highest bet-multiplier when calculating its Top Award.
        /// </summary>
        public bool NextToMaxBetTopAwardMultiplier { get; set; }
    }
}