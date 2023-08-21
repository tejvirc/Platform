namespace Aristocrat.Monaco.Sas.VoucherValidation
{
    using System.Collections.Generic;
    using Accounting.Contracts;
    using Contracts.Client;

    /// <summary>
    ///     The SAS voucher validation constants
    /// </summary>
    public static class VoucherValidationConstants
    {
        /// <summary>The key used by the client for the establishment name on tickets.</summary>
        public const string TicketLocationKey = "TicketProperty.TicketTextLine1";

        /// <summary>The key used by the client for the address on tickets.</summary>
        public const string TicketAddressLine1Key = "TicketProperty.TicketTextLine2";

        /// <summary>The key used by the client for the address on tickets.</summary>
        public const string TicketAddressLine2Key = "TicketProperty.TicketTextLine3";

        internal static readonly IDictionary<AccountType, TicketType> AccountTypeToTicketTypeMap = new Dictionary<AccountType, TicketType>
        {
            { AccountType.Cashable, TicketType.CashOut },
            { AccountType.Promo, TicketType.CashOut },
            { AccountType.NonCash, TicketType.Restricted }
        };
    }
}
