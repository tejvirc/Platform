namespace Aristocrat.Monaco.Bingo.UI.Models
{
    using System;
    using Application.Contracts.Localization;
    using Common;
    using Localization.Properties;

    /// <summary>
    ///     Settings for bingo attract
    /// </summary>
    [Serializable]
    public class BingoAttractSettings
    {
        /// <summary>
        ///     Gets or sets whether or not the attract mode should cycle the bingo patterns
        /// </summary>
        public bool CyclePatterns { get; set; }

        /// <summary>
        ///     Gets or sets whether or not the amounts should be displayed in credits
        /// </summary>
        public bool DisplayAmountsAsCredits { get; set; } = true;

        /// <summary>
        ///     Gets or sets the pattern cycle time in milliseconds
        /// </summary>
        public long PatternCycleTimeMilliseconds { get; set; } = 5000;

        /// <summary>
        ///     Gets or sets the pay out formatting text
        /// </summary>
        public string PayAmountFormattingText { get; set; } =
            Localizer.For(CultureFor.Player).GetString(ResourceKeys.BingoAttractPayFmt);

        /// <summary>
        ///     Gets or sets the bet amount formatting text
        /// </summary>
        public string BetAmountFormattingText { get; set; } =
            Localizer.For(CultureFor.Player).GetString(ResourceKeys.BingoAttractBetFmt);

        /// <summary>
        ///     Gets or sets the balls called within formatting text
        /// </summary>
        public string BallsCalledWithinFormattingText { get; set; } =
            Localizer.For(CultureFor.Player).GetString(ResourceKeys.BingoAttractBallsWithInFmt);

        /// <summary>
        ///     Gets or sets the overlay scene
        /// </summary>
        public string OverlayScene { get; set; } = BingoConstants.DefaultInitialOverlayScene;
    }
}