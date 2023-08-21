namespace Aristocrat.Monaco.Bingo.Common.Storage.Model
{
    using System.ComponentModel;

    public enum JackpotDetermination
    {
        /// <summary>
        ///     An unknown jackpot determination strategy.
        ///     This strategy should never be used and only occur when we have not received the configuration from the server
        /// </summary>
        [Description("Unknown")]
        Unknown = 0,

        /// <summary>
        ///     Strategy used when each pattern is evaluated separately against the Jackpot limit.
        ///     At most two payments will be made
        /// </summary>
        [Description("Interim Patterns")]
        InterimPattern,

        /// <summary>
        ///     Strategy used when the total win amount is compared against jackpot limit.
        ///     Only one payment is made.
        /// </summary>
        [Description("Total Wins")]
        TotalWins,

        /// <summary>
        ///     The maximum value for JackpotDetermination
        ///     This strategy should never be used
        /// </summary>
        MaxJackpotDetermination
    }
}