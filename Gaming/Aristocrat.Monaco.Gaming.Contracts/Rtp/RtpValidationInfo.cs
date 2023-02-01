namespace Aristocrat.Monaco.Gaming.Contracts.Rtp
{
    /// <summary>
    ///     After RTP validation, this class is returned, giving the basic pass or fail information of the validation.
    /// </summary>
    public class RtpValidationInfo
    {
        /// <summary>
        ///     Is <c>true</c> if the RTP is valid/legal; otherwise, <c>false</c>.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        ///     Gets or sets the type of the validation failure.
        /// </summary>
        public ValidationFailureType ValidationFailureType { get; set; }
    }
}