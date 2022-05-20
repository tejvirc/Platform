namespace Aristocrat.Monaco.Bingo.Common.Storage.Model
{
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum  PaytableEvaluation
    {
        /// <summary>
        ///     An unknown paytable evaluation method.
        ///     This method should never be used and only occur when we have not received the configuration from the server
        /// </summary>
        [Description("Unknown")]
        Unknown = 0,

        /// <summary>
        ///     Method used when the highest win pattern is awarded to the player.
        /// </summary>
        [Description("Highest Pattern Paid")]
        HPP,

        /// <summary>
        ///     Method used when all the win patterns are awarded to the player.
        /// </summary>
        [Description("All Patterns Paid")]
        APP,

        /// <summary>
        ///     The maximum value for PaytableEvaluation
        ///     This method should never be used
        /// </summary>
        MaxPaytableMethod
    }
}
