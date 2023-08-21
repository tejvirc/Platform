namespace Aristocrat.Monaco.Bingo.Common.Storage.Model
{
    using System.ComponentModel;

    public enum GameEndWinStrategy
    {
        /// <summary>
        ///     A game end win strategy that awards a free card
        /// </summary>
        [Description("Free Card")]
        FreeCard = 0,

        /// <summary>
        ///     A game end win strategy that awards a prize from a lockup table
        /// </summary>
        [Description("Credits from Table")]
        CreditsFromTable,

        /// <summary>
        ///     A game end win strategy that pays out a bonus pattern
        /// </summary>
        [Description("Chance at Bonus Pattern")]
        BonusPattern,

        /// <summary>
        ///     A game end win strategy that will bonus the credits to the play.
        ///     Outward facing name is one cent as this is the existing name used.
        /// </summary>
        [Description("One Cent")]
        BonusCredits,

        /// <summary>
        ///     A game end win strategy that pays one cent per player in the game
        /// </summary>
        [Description("One Cent Per Player")]
        OneCentPerPlayer,

        /// <summary>
        ///     The maximum value for GameEndWinStrategy
        ///     This strategy should never be used
        /// </summary>
        [Description("Unknown")]
        Unknown = int.MaxValue
    }
}