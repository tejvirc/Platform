namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    public enum ReceiveValidationNumberStatus
    {
        CommandAcknowledged = 0x00,
        NotInCashout = 0x80,
        ImproperValidationRejected = 0x81
    }

    /// <inheritdoc />
    public class ReceiveValidationNumberData : LongPollData
    {
        /// <summary>
        ///     Gets or sets the validation system ID
        /// </summary>
        public byte ValidationSystemId { get; set; }

        /// <summary>
        ///     Gets or sets the validation number
        /// </summary>
        public ulong ValidationNumber { get; set; }
    }

    /// <inheritdoc />
    public class ReceiveValidationNumberResult : LongPollResponse
    {
        /// <summary>
        ///     Creates a ReceiveValidationNumberResult instance
        /// </summary>
        /// <param name="validResponse">Whether or not the response is valid</param>
        /// <param name="status">The status to be returned</param>
        public ReceiveValidationNumberResult(bool validResponse, ReceiveValidationNumberStatus status)
        {
            ValidResponse = validResponse;
            Status = status;
        }

        /// <summary>
        ///     Gets whether or not the response is valid
        /// </summary>
        public bool ValidResponse { get; }

        /// <summary>
        ///     Gets the validation number status
        /// </summary>
        public ReceiveValidationNumberStatus Status { get; }
    }
}