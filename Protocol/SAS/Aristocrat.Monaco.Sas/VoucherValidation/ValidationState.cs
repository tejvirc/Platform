namespace Aristocrat.Monaco.Sas.VoucherValidation
{
    /// <summary>
    ///     The Host validation states
    /// </summary>
    public enum ValidationState
    {
        /// <summary>
        ///     Validation state when there is no pending validation information
        /// </summary>
        NoValidationPending,
        /// <summary>
        ///     Validation state when there is pending unread cashout information
        /// </summary>
        CashoutInformationPending,
        /// <summary>
        ///     Validation state when the validation number is waiting to be received
        /// </summary>
        ValidationNumberPending
    }
}