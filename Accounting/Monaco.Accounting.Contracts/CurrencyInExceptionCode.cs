namespace Aristocrat.Monaco.Accounting.Contracts
{
    /// <summary>
    ///     Definition of the CurrencyInExceptionCode enum
    /// </summary>
    public enum CurrencyInExceptionCode
    {
        /// <summary>
        ///     None
        /// </summary>
        None,        
       
        /// <summary>
        ///     A virtual note was inserted (keyboard command)
        /// </summary>
        Virtual,

        /// <summary>
        ///     Bill would cause credit limit to be exceeded
        /// </summary>
        CreditLimitExceeded,

        /// <summary>
        ///     Bill would cause credits in limit to be exceeded
        /// </summary>
        CreditInLimitExceeded,

        /// <summary>
        ///     Bill would cause laundry limit to be exceeded
        /// </summary>
        LaundryLimitExceeded,

        /// <summary>
        ///     Invalid note denomination
        /// </summary>
        InvalidBill,

        /// <summary>
        ///     Unable to identify inserted document
        /// </summary>
        UnknownDocument,

        /// <summary>
        ///     Note acceptor failure,
        /// </summary>
        NoteAcceptorFailure = 97,

        /// <summary>
        ///     Power failure during bill acceptance
        /// </summary>
        PowerFailure = 98,

        /// <summary>
        ///     Other
        /// </summary>
        Other = 99
    }
}
