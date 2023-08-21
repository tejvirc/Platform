namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    /// <summary>
    ///     The redemption status code
    /// </summary>
    public enum RedemptionStatusCode
    {
        /// <summary>
        ///     Cashable Ticket Redeemed
        /// </summary>
        CashableTicketRedeemed = 0x00,

        /// <summary>
        ///     Restricted Promotional ticket redeemed
        /// </summary>
        RestrictedPromotionalTicketRedeemed = 0x01,

        /// <summary>
        /// Non restricted promotional ticket redeemed
        /// </summary>
        NonRestrictedPromotionalTicketRedeemed = 0x02,

        /// <summary>
        ///     Waiting for long poll 71
        /// </summary>
        WaitingForLongPoll71 = 0x20,

        /// <summary>
        ///     Ticket Redemption pending
        /// </summary>
        TicketRedemptionPending = 0x40,

        /// <summary>
        ///     Ticket rejected by host or unknown
        /// </summary>
        TicketRejectedByHost = 0x80,

        /// <summary>
        ///     Validation numbers do not match
        /// </summary>
        ValidationNumberDoesNotMatch = 0x81,

        /// <summary>
        ///     Not a valid transfer function.
        /// </summary>
        NotAValidTransferFunction = 0x82,

        /// <summary>
        ///     Not a valid transfer amount.
        /// </summary>
        NotAValidTransferAmount = 0x83,

        /// <summary>
        ///     Transfer amount exceeded game limit.
        /// </summary>
        TransferAmountExceededGameLimit = 0x84,

        /// <summary>
        ///     Transfer amount not an even multiple.
        /// </summary>
        TransferAmountNotEvenMultiple = 0x85,

        /// <summary>
        ///     Transfer amount does not match ticket.
        /// </summary>
        TransferAmountDoesNotMatchTicket = 0x86,

        /// <summary>
        ///     Gaming machine unable to accept transfer.
        /// </summary>
        GamingMachineUnableToAcceptTransfer = 0x87,

        /// <summary>
        ///     Ticket rejected due to timeout.
        /// </summary>
        TicketRejectedDueToTimeout = 0x88,

        /// <summary>
        ///     Ticket rejected due to communication link down.
        /// </summary>
        TicketRejectedDueToCommunicationLinkDown = 0x89,

        /// <summary>
        ///     Ticket redemption disabled.
        /// </summary>
        TicketRedemptionDisabled = 0x8A,

        /// <summary>
        ///     Ticket rejected due to validator failure.
        /// </summary>
        TicketRejectedDueToValidatorFailure = 0x8B,

        /// <summary>
        ///     Not compatible with current redemption cycle.
        /// </summary>
        NotCompatibleWithCurrentRedemptionCycle = 0xc0,

        /// <summary>
        ///     No valid ticket info available.
        /// </summary>
        NoValidationInfoAvailable = 0xff,
    }
}