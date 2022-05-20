namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     Defines a fatal error for a game round
    /// </summary>
    public enum GameErrorCode
    {
        /// <summary>
        ///     No error
        /// </summary>
        None,

        /// <summary>
        ///     The win amount has exceeded the liability limit
        /// </summary>
        LiabilityLimit,

        /// <summary>
        ///     The win amount has exceeded the legitimacy limit
        /// </summary>
        LegitimacyLimit = 2,
    }
}