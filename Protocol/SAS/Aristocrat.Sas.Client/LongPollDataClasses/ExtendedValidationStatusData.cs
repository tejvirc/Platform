namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    /// <inheritdoc />
    public class ExtendedValidationStatusData : LongPollData
    {
        /// <summary>
        ///     Gets or sets the control mask used to determine what status fields to update
        /// </summary>
        public ValidationControlStatus ControlMask { get; set; }

        /// <summary>
        ///     Gets or sets the control status value used when updating
        /// </summary>
        public ValidationControlStatus ControlStatus { get; set; }

        /// <summary>
        ///     Gets or sets the cashable expiration date to set
        /// </summary>
        public int CashableExpirationDate { get; set; }

        /// <summary>
        ///     Gets or sets the restricted expiration date to set
        /// </summary>
        public int RestrictedExpirationDate { get; set; }
    }

    /// <inheritdoc />
    public class ExtendedValidationStatusResponse : LongPollResponse
    {
        /// <summary>
        ///     Create the ExtendedValidationStatusResponse
        /// </summary>
        /// <param name="assertNumber">The asset number</param>
        /// <param name="controlStatus">The current control status</param>
        /// <param name="cashableExpirationDate">The current cashable expiration date</param>
        /// <param name="restrictedExpirationDate">The current restricted expiration date</param>
        public ExtendedValidationStatusResponse(
            ulong assertNumber,
            ValidationControlStatus controlStatus,
            int cashableExpirationDate,
            int restrictedExpirationDate)
        {
            AssertNumber = assertNumber;
            ControlStatus = controlStatus;
            CashableExpirationDate = cashableExpirationDate;
            RestrictedExpirationDate = restrictedExpirationDate;
        }

        /// <summary>
        ///     Gets the asset number
        /// </summary>
        public ulong AssertNumber { get; }

        /// <summary>
        ///     Gets the control status
        /// </summary>
        public ValidationControlStatus ControlStatus { get; }

        /// <summary>
        ///     Gets the cashable expiration date
        /// </summary>
        public int CashableExpirationDate { get;  }

        /// <summary>
        ///     Gets the restricted expiration date
        /// </summary>
        public int RestrictedExpirationDate { get;  }
    }
}