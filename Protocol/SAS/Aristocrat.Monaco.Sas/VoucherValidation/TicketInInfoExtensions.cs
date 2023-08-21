namespace Aristocrat.Monaco.Sas.VoucherValidation
{
    using Accounting.Contracts.Vouchers;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;

    /// <summary>
    ///     Extension methods for the TicketInInfo
    /// </summary>
    public static class TicketInInfoExtensions
    {
        /// <summary>
        ///     Gets the exception code from the provide TicketInInfo
        /// </summary>
        /// <param name="info">The information to generate the exception code for</param>
        /// <returns>The <see cref="VoucherInExceptionCode"/> that is a result of the TicketInIno</returns>
        public static VoucherInExceptionCode GetExceptionCode(this TicketInInfo info)
        {
            switch (info.RedemptionStatusCode)
            {
                case RedemptionStatusCode.TicketRejectedByHost:
                    return GetHostExceptionCode(info.TransferCode);
                case RedemptionStatusCode.ValidationNumberDoesNotMatch:
                case RedemptionStatusCode.NotAValidTransferFunction:
                case RedemptionStatusCode.NotAValidTransferAmount:
                case RedemptionStatusCode.TransferAmountNotEvenMultiple:
                case RedemptionStatusCode.TransferAmountDoesNotMatchTicket:
                case RedemptionStatusCode.GamingMachineUnableToAcceptTransfer:
                case RedemptionStatusCode.TicketRejectedDueToCommunicationLinkDown:
                case RedemptionStatusCode.TicketRedemptionDisabled:
                case RedemptionStatusCode.TicketRejectedDueToValidatorFailure:
                case RedemptionStatusCode.NotCompatibleWithCurrentRedemptionCycle:
                case RedemptionStatusCode.NoValidationInfoAvailable:
                    return VoucherInExceptionCode.ValidationFailed;
                case RedemptionStatusCode.TicketRejectedDueToTimeout:
                    return VoucherInExceptionCode.TimedOut;
                case RedemptionStatusCode.TransferAmountExceededGameLimit:
                    return VoucherInExceptionCode.CreditInLimitExceeded;
                default:
                    return VoucherInExceptionCode.None;
            }
        }

        private static VoucherInExceptionCode GetHostExceptionCode(this TicketTransferCode transferCode)
        {
            switch (transferCode)
            {
                case TicketTransferCode.UnableToValidate:
                case TicketTransferCode.NotAValidValidationNumber:
                case TicketTransferCode.ValidationNumberNotInSystem:
                case TicketTransferCode.TicketMarkedPendingInSystem:
                case TicketTransferCode.TicketAmountExceedsAutoRedemptionLimit:
                case TicketTransferCode.ValidationInfoNotAvailable:
                case TicketTransferCode.TicketAmountDoesNotMatchSystem:
                    return VoucherInExceptionCode.ValidationFailed;
                case TicketTransferCode.TicketAlreadyRedeemed:
                    return VoucherInExceptionCode.AlreadyReedemed;
                case TicketTransferCode.TicketExpired:
                    return VoucherInExceptionCode.Expired;
                case TicketTransferCode.TicketNotValidAtThisTime:
                case TicketTransferCode.TicketNotValidOnThisGamingMachine:
                case TicketTransferCode.TicketNotValidForCurrentPlayer:
                    return VoucherInExceptionCode.InvalidTicket;
                case TicketTransferCode.PlayerCardMustBeInserted:
                    return VoucherInExceptionCode.PlayerCardMustBeInserted;
                default:
                    return VoucherInExceptionCode.None;
            }
        }
    }
}