namespace Aristocrat.Monaco.Gaming.Contracts.Bonus
{
    using System;
    using Hardware.Contracts.IdReader;

    /// <summary>
    ///     Provides a mechanism to request a bonus
    /// </summary>
    public abstract class BonusRequest : IBonusRequest
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BonusRequest" /> class.
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
        /// <param name="protocol">The related protocol tha the Request works on</param>
        protected BonusRequest(
            string bonusId,
            long cashableAmount,
            long nonCashAmount,
            long promoAmount,
            PayMethod payMethod,
            BonusException exception,
            CommsProtocol protocol)
        {
            BonusId = bonusId;
            CashableAmount = cashableAmount;
            NonCashAmount = nonCashAmount;
            PromoAmount = promoAmount;
            PayMethod = payMethod;
            Exception = exception;

            MessageDuration = TimeSpan.MaxValue;
            Mode = BonusMode.Standard;
            Protocol = protocol;
        }

        /// <summary>
        ///     Gets the bonus identifier
        /// </summary>
        public string BonusId { get; }

        /// <summary>
        ///     Gets the Cashable Amount.
        /// </summary>
        public long CashableAmount { get; }

        /// <summary>
        ///     Gets the Non Cash Amount.
        /// </summary>
        public long NonCashAmount { get; }

        /// <summary>
        ///     Gets the Promo Amount.
        /// </summary>
        public long PromoAmount { get; }

        /// <summary>
        ///     Gets the payment method
        /// </summary>
        public PayMethod PayMethod { get; }

        /// <summary>
        ///     Gets the bonus exception
        /// </summary>
        public BonusException Exception { get; }

        /// <summary>
        ///     Gets or sets the bonus message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        ///     Gets or sets the message duration
        /// </summary>
        public TimeSpan MessageDuration { get; set; }

        /// <summary>
        ///     Gets or sets the mode
        /// </summary>
        public BonusMode Mode { get; protected set; }

        /// <summary>
        ///     Gets or sets a value indicating whether a player id is required
        /// </summary>
        public bool IdRequired { get; set; }

        /// <summary>
        ///     Gets or sets the required id reader type
        /// </summary>
        public IdReaderTypes IdReaderType { get; set; }

        /// <summary>
        ///     Gets or sets the card id number
        /// </summary>
        public string IdNumber { get; set; }

        /// <summary>
        ///     Gets or sets the player id
        /// </summary>
        public string PlayerId { get; set; }

        /// <inheritdoc />
        public string SourceID { get; set; }

        /// <inheritdoc />
        public int JackpotNumber { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the eligibility timer will be evaluated
        /// </summary>
        public bool OverrideEligibility { get; set; } = true;

        /// <summary>
        ///     Gets or sets the eligibility timer used to determine whether or not the bonus has been paid
        /// </summary>
        public TimeSpan EligibilityTimer { get; set; }

        /// <summary>
        /// Gets or sets the protocol that the current request should be working on.
        /// </summary>
        public CommsProtocol Protocol { get; set; }
    }
}