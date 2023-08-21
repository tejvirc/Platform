namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    /// <inheritdoc />
    public class SendValidationMetersResponse : LongPollResponse
    {
        /// <summary>
        ///     Creates the SendValidationMetersResponse
        /// </summary>
        /// <param name="validationCount">The validation count</param>
        /// <param name="validationTotalAmount">The validation total amount</param>
        public SendValidationMetersResponse(long validationCount, long validationTotalAmount)
        {
            ValidationCount = validationCount;
            ValidationTotalAmount = validationTotalAmount;
        }

        /// <summary>
        ///     Gets or sets the validation count
        /// </summary>
        public long ValidationCount { get; }

        /// <summary>
        ///     Gets or sets the validation total amount
        /// </summary>
        public long ValidationTotalAmount { get; }
    }
}