namespace Aristocrat.Monaco.Accounting.Contracts
{
    /// <summary>
    ///     Defines the current state of an inserted note
    /// </summary>
    public enum CurrencyState
    {
        /// <summary>
        ///     Note is pending acceptance
        /// </summary>
        Pending,

        /// <summary>
        ///     Note is good, attempting to stack
        /// </summary>
        Accepting,        
        
        /// <summary>
        ///     Note was accepted
        /// </summary>
        Accepted,

        /// <summary>
        ///     Note was rejected
        /// </summary>
        Rejected,

        /// <summary>
        ///     Note was returned
        /// </summary>
        Returned
    }
}
