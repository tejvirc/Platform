namespace Aristocrat.Monaco.Gaming.Contracts.Rtp
{
    /// <summary>
    ///     After RTP validation, this class is returned, giving the basic pass or fail information of the validation.
    /// </summary>
    public class RtpValidationInfo
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RtpValidationInfo" /> class.
        /// </summary>
        /// <param name="isValid">if set to <c>true</c> [is RTP valid].</param>
        /// <param name="failureType">Type of the failure.</param>
        public RtpValidationInfo(bool isValid, ValidationFailureType failureType = ValidationFailureType.None)
        {
            IsValid = isValid;
            ValidationFailureType = failureType;
        }

        /// <summary>
        ///     Is <c>true</c> if the RTP is valid/legal; otherwise, <c>false</c>.
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        ///     Gets or sets the type of the validation failure.
        /// </summary>
        public ValidationFailureType ValidationFailureType { get; set; }
    }
}