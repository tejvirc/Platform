namespace Aristocrat.Monaco.Gaming.Contracts.Bonus
{
    using System.ComponentModel;

    /// <summary>
    ///     Defines the bonus mode
    /// </summary>
    public enum BonusMode
    {
        /// <summary>
        ///     Standard bonus award mode
        /// </summary>
        [Description("Standard")]
        Standard,

        /// <summary>
        ///     Non-Deductible
        /// </summary>
        [Description("Non-Deductible")]
        NonDeductible,

        /// <summary>
        ///     Wager Match mode
        /// </summary>
        [Description("Wager Match")]
        WagerMatch,

        /// <summary>
        ///     Multiple Jackpot Time mode
        /// </summary>
        [Description("Multiple Jackpot Time")]
        MultipleJackpotTime,

        /// <summary>
        ///     Wager Match in Full mode
        /// </summary>
        [Description("Wager Match")]
        WagerMatchAllAtOnce,

        /// <summary>
        ///     Paytable Awarded Game Win Bonus
        /// </summary>
        [Description("Game Win")]
        GameWin
    }
}