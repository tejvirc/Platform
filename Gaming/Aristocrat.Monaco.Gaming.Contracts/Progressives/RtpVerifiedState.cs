namespace Aristocrat.Monaco.Gaming.Contracts.Progressives
{
    using System.ComponentModel;

    /// <summary>
    ///     The RTP verified state.
    /// </summary>
    public enum RtpVerifiedState
    {
        /// <summary>
        ///     RTP is verified.
        /// </summary>
        [Description("Verified")]
        Verified = 0,

        /// <summary>
        ///     RTP is not verified.
        /// </summary>
        [Description("Not Verified")]
        NotVerified,

        /// <summary>
        ///     RTP is not used. E.g. It's disabled in Gaming.config.xml.
        /// </summary>
        [Description("Not Used")]
        NotUsed,

        /// <summary>
        ///     RTP is not available.
        /// </summary>
        [Description("Not Available")]
        NotAvailable
    }
}