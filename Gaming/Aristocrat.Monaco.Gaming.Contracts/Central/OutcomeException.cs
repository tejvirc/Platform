namespace Aristocrat.Monaco.Gaming.Contracts.Central
{
    /// <summary>
    ///     Defines exceptions for outcomes
    /// </summary>
    public enum OutcomeException
    {
        /// <summary>
        ///     No exceptions
        /// </summary>
        None,

        /// <summary>
        ///     Awaiting Outcome
        /// </summary>
        Pending,

        /// <summary>
        ///     Request timed out
        /// </summary>
        TimedOut,

        /// <summary>
        ///     Game changed
        /// </summary>
        GameChanged,

        /// <summary>
        ///     Invalid outcome received
        /// </summary>
        Invalid,

        /// <summary>
        ///     Unable to display outcome
        /// </summary>
        DisplayFailed,

        /// <summary>
        ///     Invalid progressive index
        /// </summary>
        InvalidProgressiveIndex,

        /// <summary>
        ///     Invalid paytable index
        /// </summary>
        InvalidPaytableIndex
    }
}