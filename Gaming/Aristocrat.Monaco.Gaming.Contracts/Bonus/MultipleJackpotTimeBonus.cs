namespace Aristocrat.Monaco.Gaming.Contracts.Bonus
{
    using System;
    using Accounting.Contracts;

    /// <summary>
    ///     Player wager limitations
    /// </summary>
    public enum WagerRestriction
    {
        /// <summary>
        ///     Restricted to max bet
        /// </summary>
        MaxBet,

        /// <summary>
        ///     Restricted to current bet
        /// </summary>
        CurrentBet
    }

    /// <summary>
    ///     Timeout rules
    /// </summary>
    public enum TimeoutRule
    {
        /// <summary>
        ///     Auto Start
        /// </summary>
        AutoStart,

        /// <summary>
        ///     Exit Mode
        /// </summary>
        ExitMode,

        /// <summary>
        ///     Ignore
        /// </summary>
        Ignore
    }

    /// <summary>
    ///     Used to authorize a multiple jackpot time bonus
    /// </summary>
    public class MultipleJackpotTimeBonus : BonusRequest
    {
        // Used to denote the credit type is being used
        private const int UseCreditType = -1;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BonusRequest" /> class.
        /// </summary>
        /// <param name="bonusId">The bonus identifier</param>
        /// <param name="accountType">Specifies how to pay the award</param>
        /// <param name="payMethod">The pay method</param>
        /// <param name="exception">
        ///     Allows the requestor to define an exception due to an invalid request. The transaction will be
        ///     created and then terminated when the exception is not None
        /// </param>
        /// <param name="protocol">The related protocol</param>
        public MultipleJackpotTimeBonus(string bonusId, AccountType accountType, PayMethod payMethod, BonusException exception = BonusException.None, CommsProtocol protocol = CommsProtocol.None)
            : base(
                bonusId,
                accountType == AccountType.Cashable ? UseCreditType : 0,
                accountType == AccountType.NonCash ? UseCreditType : 0,
                accountType == AccountType.Promo ? UseCreditType : 0,
                payMethod,
                exception,
                protocol)
        {
            Mode = BonusMode.MultipleJackpotTime;
        }

        /// <summary>
        ///     Gets the payout type associated with this bonus
        /// </summary>
        public AccountType AccountType => CashableAmount == UseCreditType ? AccountType.Cashable :
            NonCashAmount == UseCreditType ? AccountType.NonCash : AccountType.Promo;

        /// <summary>
        ///     Gets or sets the start datetime of the bonus
        /// </summary>
        public DateTime? Start { get; set; }

        /// <summary>
        ///     Gets or sets the end datetime of the bonus
        /// </summary>
        public DateTime? End { get; set; }

        /// <summary>
        ///     Gets or sets the number of games that this mode can be applied to
        /// </summary>
        public int Games { get; set; }

        /// <summary>
        ///     Gets or sets the timeout value
        /// </summary>
        public TimeSpan Timeout { get; set; }

        /// <summary>
        ///     Gets or sets timeout rule
        /// </summary>
        public TimeoutRule TimeoutRule { get; set; }

        /// <summary>
        ///     Gets or sets the Multiplier value to multiply wins by
        /// </summary>
        public int WinMultiplier { get; set; }

        /// <summary>
        ///     Gets or sets the smallest win, inclusive, that is eligible to be multiplied
        /// </summary>
        public long MinimumWin { get; set; }

        /// <summary>
        ///     Gets or sets the maximum win, inclusive, that is eligible to be multiplied
        /// </summary>
        public long MaximumWin { get; set; }

        /// <summary>
        ///     Gets or sets how to limit the wagers for the games played
        /// </summary>
        public WagerRestriction WagerRestriction { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether or not the bonus is rejected for low credits
        /// </summary>
        public bool RejectLowCredits { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether or not the bonus is ended on card out
        /// </summary>
        public bool EndOnCardOut { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether or not the bonus is ended on cash out
        /// </summary>
        public bool EndOnCashOut { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether or not autoplay is enabled
        /// </summary>
        public bool AutoPlay { get; set; }
    }
}