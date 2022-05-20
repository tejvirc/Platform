namespace Aristocrat.Monaco.Gaming.Contracts.Bonus
{
    using System;

    /// <summary>
    ///     Used to authorize a wager match bonus
    /// </summary>
    public class WagerMatchBonus : BonusRequest, IAwardLimit
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="WagerMatchBonus" /> class.
        /// </summary>
        /// <param name="bonusId">The bonus identifier</param>
        /// <param name="cashableAmount">The cashable amount</param>
        /// <param name="nonCashAmount">The non cashable amount</param>
        /// <param name="promoAmount">The promotional amount</param>
        /// <param name="payMethod">The pay method</param>
        /// <param name="exception">
        ///     Allows the requestor to define an exception due to an invalid request. The transaction will be
        ///     created and then terminated when the exception is not None
        /// </param>
        /// <param name="protocol">The related protocol</param>
        public WagerMatchBonus(string bonusId, long cashableAmount, long nonCashAmount, long promoAmount, PayMethod payMethod, BonusException exception = BonusException.None, CommsProtocol protocol = CommsProtocol.None)
            : base(bonusId, cashableAmount, nonCashAmount, promoAmount, payMethod, exception, protocol)
        {
            Mode = BonusMode.WagerMatch;
        }

        /// <summary>
        ///     Gets the display limit for the bonus.  If the amount exceeds the limit the bonus will fail.  A value of 0 disables the check
        /// </summary>
        public long DisplayLimit { get; set; }

        /// <summary>
        ///     Gets the display limit text for the bonus that will be displayed based if the display limit validation fails
        /// </summary>
        public string DisplayLimitText { get; set; }

        /// <summary>
        ///     Gets the display limit text for the bonus that will be displayed based if the display limit validation fails
        /// </summary>
        public TimeSpan DisplayLimitTextDuration { get; set; } = TimeSpan.Zero;
    }
}