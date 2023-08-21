namespace Aristocrat.Monaco.Sas.VoucherValidation
{
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Aristocrat.Sas.Client.LongPollDataClasses;

    /// <summary>
    ///     Handler for host validation
    /// </summary>
    public interface IEnhancedValidationProvider
    {
        /// <summary>
        ///     Gets the response from the information
        /// </summary>
        /// <param name="data">The send enhanced validation information</param>
        /// <returns>The send enhanced validation information response</returns>
        SendEnhancedValidationInformationResponse GetResponseFromInfo(SendEnhancedValidationInformation data);

        /// <summary>
        ///     Handles the hand pay transaction after hand pay is reset
        /// </summary>
        /// <param name="transaction">The hand pay transaction</param>
        void HandPayReset(HandpayTransaction transaction);

        /// <summary>
        ///     Handles the voucher out transaction after voucher is issued
        /// </summary>
        /// <param name="transaction">The voucher out transaction</param>
        void HandleTicketOutCompleted(VoucherOutTransaction transaction);
    }
}