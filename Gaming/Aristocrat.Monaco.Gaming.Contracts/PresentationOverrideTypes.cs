namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     Types of presentation the game may register to override
    /// </summary>
    public enum PresentationOverrideTypes
    {
        /// <summary>
        ///     Cashout Ticket being printed
        /// </summary>
        PrintingCashoutTicket = 0,

        /// <summary>
        ///     Cashwin Ticket being printed
        /// </summary>
        PrintingCashwinTicket = 1,

        /// <summary>
        ///     Credits being transferred in
        /// </summary>
        TransferingInCredits = 2,

        /// <summary>
        ///     Credits being transferred out
        /// </summary>
        TransferingOutCredits = 3,

        /// <summary>
        ///     Jackpot handpay occurring
        /// </summary>
        JackpotHandpay = 4,

        /// <summary>
        ///     Bonus Jackpot occurring
        /// </summary>
        BonusJackpot = 5,

        /// <summary>
        ///     Cancelled credits handpay occurring
        /// </summary>
        CancelledCreditsHandpay = 6,
    }
}
