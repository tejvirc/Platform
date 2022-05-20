namespace Aristocrat.Monaco.Hhr.Client.Messages
{
    /// <summary>
    ///     Transaction types for HHR message of type CMD_TRANSACTION
    ///     <see cref="TransactionRequest" />
    /// </summary>
    public enum CommandTransactionType
    {
        /// <summary> Unknown transaction </summary>
        Unknown = -1,

        /// <summary> Bills inserted </summary>
        BillInserted,

        /// <summary> Ticket is inserted </summary>
        TicketInserted,

        /// <summary> Ticket printed </summary>
        TicketPrinted,

        /// <summary> Game is played </summary>
        GamePlayed,

        /// <summary> Progressive win </summary>
        ProgWon,

        /// <summary> Game win </summary>
        GameWon,

        /// <summary> Player card inserted </summary>
        CardIn,

        /// <summary> Player card removed </summary>
        CardOut,

        /// <summary> Cancelled credits </summary>
        CreditsCancelled,

        //	HandpayTicketPrinted,

        /// <summary> Credit conversion </summary>
        CreditConversion,

        /// <summary> Promo ticket inserted </summary>
        TicketInsertedPromo,

        /// <summary> Non-cashable ticket inserted </summary>
        TicketInsertedNonCashable,

        /// <summary> Non-cashable ticket printed </summary>
        TicketPrintedNonCashable,

        /// <summary> Promo ticket printed </summary>
        TicketPrintedPromo,

        //	HandpayWinNoReceipt,

        /// <summary> Electronic cashable amount inserted </summary>
        AftInCashable,

        /// <summary> Electronic promo amount inserted </summary>
        AftInPromo,

        /// <summary> Electronic non-cashable amount inserted </summary>
        AftInNonCashable,

        /// <summary> Electronic cashable amount removed </summary>
        AftOutCashable,

        /// <summary> Electronic promo amount inserted </summary>
        AftOutPromo,

        /// <summary> Electronic non-cashable amount inserted </summary>
        AftOutNonCashable,

        /// <summary> Bonus cashable win </summary>
        BonusWinCashable,

        /// <summary> Bonus promo win </summary>
        BonusWinPromo,

        /// <summary> Bonus handpaid cash </summary>
        BonusHandpayCashable,

        /// <summary> Bonus handpaid promo </summary>
        BonusHandpayPromo,

        /// <summary> Legacy bonus awarded </summary>
        LegacyBonus,

        /// <summary> Game win handpaid with no receipt </summary>
        GameWinToHandpayNoReceipt,

        /// <summary> Game win paid to credit </summary>
        GameWinToCreditMeter,

        /// <summary> Game win paid via cashout ticket </summary>
        GameWinToCashableOutTicket,

        /// <summary> Game win transferred out  </summary>
        GameWinToAftHost,

        /// <summary> Game win handpaid with receipt </summary>
        GameWinToHandpayReceipt,

        /// <summary> Bonus win transferred out </summary>
        BonusWinToAftHost,

        /// <summary> Bonus win printed as cashable ticket</summary>
        BonusWinToCashableOutTicket,

        /// <summary> Bonus win handpaid with no receipt </summary>
        BonusWinToHandpayNoReceipt,

        /// <summary> Bonus win to credit meter</summary>
        BonusWinToCreditMeter,

        /// <summary> Bonus win handpaid with receipt </summary>
        BonusWinToHandpayReceipt,

        /// <summary> Bonus handpay amount handpaid with no receipt </summary>
        HandpayBonusToHandpayNoReceipt,

        /// <summary> Bonus handpay amount to credit meter</summary>
        HandpayBonusToCreditMeter,

        /// <summary> Bonus handpay amount handpaid with receipt </summary>
        HandpayBonusToHandpayReceipt,

        /// <summary>  </summary>
        MgamGamePlay, // For the MGAMTPS only - not processed by BGC or GT

        /// <summary> </summary>
        MgamGameWin, // For the MGAMTPS only - not processed by BGC or GT

        /// <summary> AftCashableOutTicket </summary>
        AftCashableOutTicket,

        /// <summary> AftCashableOutTicketComplete </summary>
        AftCashableOutTicketComplete
    }
}