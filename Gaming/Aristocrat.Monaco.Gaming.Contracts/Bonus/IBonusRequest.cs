namespace Aristocrat.Monaco.Gaming.Contracts.Bonus
{
    using System;
    using Hardware.Contracts.IdReader;

    /// <summary>
    ///     Provides a mechanism to request a bonus award
    /// </summary>
    public interface IBonusRequest
    {
        /// <summary>
        ///     Gets the bonus identifier
        /// </summary>
        string BonusId { get; }

        /// <summary>
        ///     Gets the Cashable Amount.
        /// </summary>
        long CashableAmount { get; }

        /// <summary>
        ///     Gets the Non Cash Amount.
        /// </summary>
        long NonCashAmount { get; }

        /// <summary>
        ///     Gets the Promo Amount.
        /// </summary>
        long PromoAmount { get; }

        /// <summary>
        ///     Gets the payment method
        /// </summary>
        PayMethod PayMethod { get; }

        /// <summary>
        ///     Gets the bonus exception
        /// </summary>
        /// <remarks>
        ///     Allows the requestor to define an exception due to an invalid request. The transaction will be created and then
        ///     terminated when the exception is not None
        /// </remarks>
        BonusException Exception { get; }

        /// <summary>
        ///     Gets or sets the bonus message
        /// </summary>
        string Message { get; set; }

        /// <summary>
        ///     Gets or sets the message duration
        /// </summary>
        TimeSpan MessageDuration { get; set; }

        /// <summary>
        ///     Gets the mode
        /// </summary>
        BonusMode Mode { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether a player id is required
        /// </summary>
        bool IdRequired { get; set; }

        /// <summary>
        ///     Gets or sets the required id reader type
        /// </summary>
        IdReaderTypes IdReaderType { get; set; }

        /// <summary>
        ///     Gets or sets the card id number
        /// </summary>
        string IdNumber { get; set; }

        /// <summary>
        ///     Gets or sets the player id
        /// </summary>
        string PlayerId { get; set; }

        /// <summary>
        ///     Gets or sets the source id
        /// </summary>
        string SourceID { get; set; }

        /// <summary>
        ///     Gets or sets the jackpot number
        /// </summary>
        int JackpotNumber { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the eligibility timer will be evaluated
        /// </summary>
        bool OverrideEligibility { get; set; }

        /// <summary>
        ///     Gets or sets the eligibility timer used to determine whether or not the bonus has been paid
        /// </summary>
        TimeSpan EligibilityTimer { get; set; }

        /// <summary>
        /// Gets or sets the protocol that the current request is working on.
        /// </summary>
        CommsProtocol Protocol { get; set; }
    }
}