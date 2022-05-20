namespace Aristocrat.Monaco.Sas.Storage.Models
{
    using System;
    using System.Collections.Generic;
    using Common.Storage;
    using Contracts;
    using Contracts.Client;

    /// <summary>
    ///     The entity for ticket storage
    /// </summary>
    public class TicketStorageData : BaseEntity, ICloneable
    {
        /// <summary> The indication that expiration isn't set </summary>
        public const int ExpirationNotSet = -1;

        /// <summary> whether redemption is enabled or not </summary>
        public bool RedemptionEnabled { get; set; } = true;

        /// <summary>
        ///     Gets or sets the voucher in state
        /// </summary>
        public SasVoucherInState VoucherInState { get; set; }

        /// <summary>
        ///     Gets or set the pool id
        /// </summary>
        public int PoolId { get; set; }

        /// <summary>
        ///     Gets or sets the ticket info field
        /// </summary>
        public string TicketInfoField { get; set; } = string.Empty;

        /// <summary> the expiration in days </summary>
        public int TicketExpiration { get; set; } = ExpirationNotSet;

        /// <summary> the expiration in days </summary>
        public int CashableTicketExpiration { get; set; } = ExpirationNotSet;

        /// <summary> the expiration in days </summary>
        public int RestrictedTicketExpiration { get; set; } = ExpirationNotSet;

        /// <summary> All expiration days/dates defined for the Restricted Ticket </summary>
        public Dictionary<ExpirationOrigin, int> RestrictedExpirationDictionary =>
            new Dictionary<ExpirationOrigin, int>
            {
                { ExpirationOrigin.EgmDefault, RestrictedTicketDefaultExpiration },
                { ExpirationOrigin.Combined, RestrictedTicketCombinedExpiration },
                { ExpirationOrigin.Independent, RestrictedTicketIndependentExpiration },
                { ExpirationOrigin.Credits, RestrictedTicketCreditsExpiration }
            };

        /// <summary>
        ///     Gets or sets the default restricted expiration date
        /// </summary>
        public int RestrictedTicketDefaultExpiration { get; set; } = ExpirationNotSet;

        /// <summary>
        ///     Gets or sets the combined restricted expiration date
        /// </summary>
        public int RestrictedTicketCombinedExpiration { get; set; } = ExpirationNotSet;

        /// <summary>
        ///     Gets or sets the independent expiration date
        /// </summary>
        public int RestrictedTicketIndependentExpiration { get; set; } = ExpirationNotSet;

        /// <summary>
        ///     Gets or sets the credits expiration date
        /// </summary>
        public int RestrictedTicketCreditsExpiration { get; set; } = ExpirationNotSet;

        /// <inheritdoc />
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}