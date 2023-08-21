namespace Aristocrat.Sas.Client
{
    /// <summary>
    ///     Represents the validation type for ticket outs.
    ///     This list was generated from the SAS manual Table 15.13c Validation Type Code Values
    /// </summary>
    public enum TicketValidationType
    {
        /// <summary>
        ///     Cashable ticket from cash out or win.
        /// </summary>
        CashableTicketFromCashOutOrWin = 0x00,

        /// <summary>
        ///     Restricted promotional cash out ticket.
        /// </summary>
        RestrictedPromotionalTicketFromCashOut = 0x01,

        /// <summary>
        ///     Cashable ticket from Aft transfer.
        /// </summary>
        CashableTicketFromAftTransfer = 0x02,

        /// <summary>
        ///     Restricted ticket from Aft transfer.
        /// </summary>
        RestrictedTicketFromAftTransfer = 0x03,

        /// <summary>
        ///     Debit ticket from Aft transfer.
        /// </summary>
        DebitTicketFromAftTransfer = 0x04,

        /// <summary>
        ///     Handpay from cashout receipt.
        /// </summary>
        HandPayFromCashOutReceiptPrinted = 0x10,

        /// <summary>
        ///     Handpay from win receipt.
        /// </summary>
        HandPayFromWinReceiptPrinted = 0x20,

        /// <summary>
        ///     Handpay from cash out no receipt.
        /// </summary>
        HandPayFromCashOutNoReceipt = 0x40,

        /// <summary>
        ///     Handapy from win no receipt.
        /// </summary>
        HandPayFromWinNoReceipt = 0x60,

        /// <summary>
        ///     Cashable ticket redeemed.
        /// </summary>
        CashableTicketRedeemed = 0x80,

        /// <summary>
        ///     Restricted promotional ticket redeemed.
        /// </summary>
        RestrictedPromotionalTicketRedeemed = 0x81,

        /// <summary>
        ///     Nonrestricted promotional ticket redeemed.
        /// </summary>
        NonRestrictedPromotionalTicketRedeemed = 0x82,

        /// <summary>
        ///     No validation type specified.
        /// </summary>
        None = 0xff,
    }
}