namespace Aristocrat.Monaco.Sas.Handlers
{
    using Accounting.Contracts;
    using Aristocrat.Monaco.Accounting.Contracts.Vouchers;
    using Aristocrat.Sas.Client.LongPollDataClasses;

    /// <summary>
    ///     Extension methods for an Voucher.
    /// </summary>
    public static class VoucherExtension
    {
        /// <summary>
        ///     Extension method for mapping VoucherInExceptionCode to RedemptionStatusCode.
        /// </summary>
        public static RedemptionStatusCode ToRedemptionStatusCode(this VoucherInExceptionCode @this)
        {
            switch (@this)
            {
                case VoucherInExceptionCode.ValidationFailed:
                case VoucherInExceptionCode.NoteAcceptorFailure:
                    return RedemptionStatusCode.TicketRejectedDueToValidatorFailure;

                case VoucherInExceptionCode.CreditLimitExceeded:
                    return RedemptionStatusCode.TransferAmountExceededGameLimit;

                case VoucherInExceptionCode.TicketingDisabled:
                    return RedemptionStatusCode.GamingMachineUnableToAcceptTransfer;
                case VoucherInExceptionCode.ZeroAmount:
                    return RedemptionStatusCode.NotAValidTransferAmount;

                default:
                    return RedemptionStatusCode.TicketRejectedByHost;
            }
        }

        /// <summary>
        ///     Converts the accounting type to the redemption status code
        /// </summary>
        /// <param name="this">The account type for this conversion</param>
        /// <returns>The redemption status code</returns>
        public static RedemptionStatusCode ToRedemptionStatusCode(this AccountType @this)
        {
            switch (@this)
            {
                case AccountType.Promo:
                    return RedemptionStatusCode.NonRestrictedPromotionalTicketRedeemed;
                case AccountType.NonCash:
                    return RedemptionStatusCode.RestrictedPromotionalTicketRedeemed;
                default:
                    return RedemptionStatusCode.CashableTicketRedeemed;
            }
        }
    }
}
