namespace Aristocrat.Monaco.Accounting.Contracts.Vouchers
{
    /// <summary>
    ///     Exception code for Voucher In rejection
    /// </summary>
    public enum VoucherInExceptionCode
    {
        /// <summary>
        ///     None
        /// </summary>
        None,        

        /// <summary>
        ///     Voucher over credit limit
        /// </summary>
        CreditLimitExceeded,        

        /// <summary>
        ///     Voucher over credit in limit
        /// </summary>
        CreditInLimitExceeded,

        /// <summary>
        ///     Voucher over laundry limit
        /// </summary>
        LaundryLimitExceeded,

        /// <summary>
        ///     Voucher over voucher in limit
        /// </summary>
        VoucherInLimitExceeded,

        /// <summary>
        ///     Validation failed
        /// </summary>
        ValidationFailed,

        /// <summary>
        ///     Voucher already redeemed
        /// </summary>
        AlreadyReedemed,

        /// <summary>
        ///     Voucher is Expired
        /// </summary>
        Expired,

        /// <summary>
        ///     Player card must be inserted
        /// </summary>
        PlayerCardMustBeInserted,

        /// <summary>
        ///     The voucher is invalid
        /// </summary>
        InvalidTicket,

        /// <summary>
        ///     The voucher amount is zero
        /// </summary>
        ZeroAmount,
        
        /// <summary>
        ///     The voucher amount is zero
        /// </summary>
        TicketingDisabled,        
        
        /// <summary>
        ///     Unknown document
        /// </summary>
        UnknownDocument,  
        
        /// <summary>
        ///     Redemption timed out
        /// </summary>
        TimedOut,

        /// <summary>
        ///     Ticket is being processed at another location
        /// </summary>
        InProcessAtAnotherLocation,

        /// <summary>
        ///     Incorrect Player
        /// </summary>
        IncorrectPlayer,

        /// <summary>
        ///     Printer presentation error
        /// </summary>
        PrinterError,

        /// <summary>
        ///     Another transfer is in progress
        /// </summary>
        AnotherTransferInProgress,

        /// <summary>
        ///     Cannot mix not cashable expired
        /// </summary>
        CannotMixNonCashableExpired,

        /// <summary>
        ///     Cannot mix not cashable credits
        /// </summary>
        CannotMixNonCashableCredits,

        /// <summary>
        ///     Note acceptor failure,
        /// </summary>
        NoteAcceptorFailure = 97,

        /// <summary>
        ///     Power failure during voucher redemption
        /// </summary>
        PowerFailure = 98,

        /// <summary>
        ///     Other
        /// </summary>
        Other = 99
    }
}