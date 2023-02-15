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
        ///     RTP is not used. E.g. It's disabled in Gaming.config.xml.
        /// </summary>
        NotUsed,

        /// <summary>
        ///     RTP is not available.
        /// </summary>
        NotAvailable
    }
}