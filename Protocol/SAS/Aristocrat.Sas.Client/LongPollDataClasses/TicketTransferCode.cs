namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    /// <summary>
    ///     It enumerates all parsing codes used in ticketing.
    /// </summary>
    public enum ParsingCode : byte
    {
        /// <summary> BCD code</summary>
        Bcd = 0x00,

        /// <summary> byte code</summary>
        Byte = 0x01,
    }

    /// <summary>
    ///     It enumerates all transfer codes received from Backend.
    /// </summary>
    public enum TicketTransferCode
    {
        /// <summary> Valid Cashable Ticket</summary>
        ValidCashableTicket = 0x00,

        /// <summary> Valid Restricted Promotional Ticket</summary>
        ValidRestrictedPromotionalTicket = 0x01,

        /// <summary> Valid NonRestricted Promotional Ticket</summary>
        ValidNonRestrictedPromotionalTicket = 0x02,

        /// <summary> Unable To Validate</summary>
        UnableToValidate = 0x80,

        /// <summary>Not A Valid Validation Number</summary>
        NotAValidValidationNumber = 0x81,

        /// <summary>Validation Number Not In System</summary>
        ValidationNumberNotInSystem = 0x82,

        /// <summary> Ticket Marked Pending In System</summary>
        TicketMarkedPendingInSystem = 0x83,

        /// <summary> Ticket Already Redeemed</summary>
        TicketAlreadyRedeemed = 0x84,

        /// <summary> Ticket Expired</summary>
        TicketExpired = 0x85,

        /// <summary> Validation Info Not Available</summary>
        ValidationInfoNotAvailable = 0x86,

        /// <summary> Ticket Amount Does Not Match System</summary>
        TicketAmountDoesNotMatchSystem = 0x87,

        /// <summary> Ticket Amount Exceeds Auto Redemption Limit</summary>
        TicketAmountExceedsAutoRedemptionLimit = 0x88,

        /// <summary> Ticket Not Valid At This Time</summary>
        TicketNotValidAtThisTime = 0x90,

        /// <summary> Ticket Not Valid On This Gaming Machine</summary>
        TicketNotValidOnThisGamingMachine = 0x91,

        /// <summary> Player Card Must Be Inserted</summary>
        PlayerCardMustBeInserted = 0x92,

        /// <summary> Ticket Not Valid For Current Player</summary>
        TicketNotValidForCurrentPlayer = 0x93,

        /// <summary> Request For Current Ticket Status</summary>
        RequestForCurrentTicketStatus = 0xFF,
    }
}
