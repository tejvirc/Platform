namespace Aristocrat.Monaco.Gaming.Contracts.Progressives
{
    /// <summary>
    ///     The RTP verified state.
    /// </summary>
    public enum RtpVerifiedState
    {
        /// <summary>
        ///     RTP is verified.
        /// </summary>
        Verified = 0,

        /// <summary>
        ///     RTP is not verified.
        /// </summary>
        NotVerified,

        /// <summary>
        ///     RTP is not used.
        /// </summary>
        NotUsed,

        /// <summary>
        ///     RTP is not available.
        /// </summary>
        NotAvailable
    }
}