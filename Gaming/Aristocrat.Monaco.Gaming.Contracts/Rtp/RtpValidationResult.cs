namespace Aristocrat.Monaco.Gaming.Contracts.Rtp
{
    /// <summary>
    ///     After RTP validation, this class is returned, providing the details of the validation.
    /// </summary>
    public class RtpValidationResult
    {
        /// <summary>
        ///     Is <c>true</c> if all the RTP values are valid & legal; otherwise, <c>false</c>.
        /// </summary>
        public bool IsValid => FailureFlags == RtpValidationFailureFlags.None;

        /// <summary>
        ///     Gets or sets the validation failure flags.
        /// </summary>
        public RtpValidationFailureFlags FailureFlags { get; set; }
    }
}