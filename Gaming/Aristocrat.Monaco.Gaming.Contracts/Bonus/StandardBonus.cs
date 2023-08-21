namespace Aristocrat.Monaco.Gaming.Contracts.Bonus
{
    using System;

    /// <summary>
    ///     Defines a standard bonus type
    /// </summary>
    public class StandardBonus : BonusRequest, IAwardLimit
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="StandardBonus" /> class.
        /// </summary>
        /// <param name="bonusId">The bonus identifier</param>
        /// <param name="cashableAmount">The cashable amount</param>
        /// <param name="nonCashAmount">The non cashable amount</param>
        /// <param name="promoAmount">The promotional amount</param>
        /// <param name="payMethod">The pay method</param>
        /// <param name="allowedWhileDisabled">Whether to allow bonus during disabled state</param>
        /// <param name="mode">The bonus mode</param>
        /// <param name="exception">
        ///     Allows the requestor to define an exception due to an invalid request. The transaction will be
        ///     created and then terminated when the exception is not None
        /// </param>
        /// <param name="protocol">The related protocol</param>
        public StandardBonus(string bonusId, long cashableAmount, long nonCashAmount, long promoAmount, PayMethod payMethod, bool allowedWhileDisabled = false, BonusMode mode = BonusMode.Standard, BonusException exception = BonusException.None, CommsProtocol protocol = CommsProtocol.None)
            : base(bonusId, cashableAmount, nonCashAmount, promoAmount, payMethod, exception, protocol)
        {
            if (mode != BonusMode.Standard && mode != BonusMode.NonDeductible && mode != BonusMode.WagerMatchAllAtOnce)
            {
                throw new ArgumentOutOfRangeException(nameof(mode));
            }

            Mode = mode;
            MessageDuration = TimeSpan.MaxValue;
            AllowedWhileDisabled = allowedWhileDisabled;
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

        /// <summary>
        ///     Some bonus types allow bonus during some lockups(ex. Legacy bonus during Handpay)
        ///     This flag will allow bonus when GamePlay state is disabled. Specific disabled state
        ///     check should be made while initiating BonusRequest.
        /// </summary>
        public bool AllowedWhileDisabled { get; set; }

        /// <summary>
        /// Gets or sets the 'allowed while in audit mode' used to determine whether or not to
        /// pay(commit) a credit transfer request while system is in audit mode. If the value is false,
        /// the request will be queued for future processing.
        /// </summary>
        public bool AllowedWhileInAuditMode { get; set; } = true;
    }
}
