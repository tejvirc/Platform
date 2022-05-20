namespace Aristocrat.Monaco.Gaming.Contracts.Bonus
{
    /// <summary>
    ///     Defines the bonus exceptions
    /// </summary>
    public enum BonusException
    {
        /// <summary>
        ///     No exception
        /// </summary>
        None = 0,

        /// <summary>
        ///     Cancelled
        /// </summary>
        Cancelled = 1,

        /// <summary>
        ///     Ineligible
        /// </summary>
        Ineligible = 2,

        /// <summary>
        ///     No PlayerId
        /// </summary>
        NoPlayerId = 3,

        /// <summary>
        ///     Invalid PlayerId
        /// </summary>
        InvalidPlayerId = 4,

        /// <summary>
        ///     Mixed NonCashable Expiration
        /// </summary>
        MixedNonCashableExpiration = 5,

        /// <summary>
        ///     Mixed NonCashable Credits
        /// </summary>
        MixedNonCashableCredits = 6,

        /// <summary>
        ///     Not playable
        /// </summary>
        NotPlayable = 7,

        /// <summary>
        ///     Pay Method Not Available
        /// </summary>
        PayMethodNotAvailable = 8,

        /// <summary>
        ///     Failed
        /// </summary>
        Failed = 99
    }

    /// <summary>
    ///     Defines additional information for <see cref="BonusException" />
    /// </summary>
    public enum BonusExceptionInfo
    {
        /// <summary>
        ///     No additional information
        /// </summary>
        None,

        /// <summary>
        ///     The bonus limit has been exceeded
        /// </summary>
        LimitExceeded,

        /// <summary>
        ///     The bonus cannot be paid due to eligibility
        /// </summary>
        Ineligible,

        /// <summary>
        ///     The award amount is not set to zero
        /// </summary>
        InvalidAwardAmount,

        /// <summary>
        ///     The wager match limit has been exceeded
        /// </summary>
        WagerMatchLimitExceeded,

        /// <summary>
        ///     MJT failed, insufficient funds
        /// </summary>
        InsufficientFunds,

        /// <summary>
        ///     MJT failed, unrestricted session
        /// </summary>
        UnrestrictedSession,

        /// <summary>
        ///     MJT failed, auto-play not allowed
        /// </summary>
        AutoPlayNotAllowed
    }
}